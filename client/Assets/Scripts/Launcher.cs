using System.Threading;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using ExitGames.Client.Photon;

using Photon.Realtime;

using Prism;

using UniRx;

using UnityEngine;

public static class UniRxInit
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Init()
    {
        MainThreadDispatcher.Initialize();
    }
}

public class Launcher: MonoBehaviour
{
    private readonly string gameVersion = "1";

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 4;

    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;

    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    public void Awake()
    {
        GameObject.DontDestroyOnLoad(this);
    }

    public void Start()
    {
        this.progressLabel.SetActive(false);
        this.controlPanel.SetActive(true);
    }

    private PrismClient client;

    public void Connect()
    {
        this.progressLabel.SetActive(true);
        this.controlPanel.SetActive(false);

        Debug.LogFormat("Main: {0}", Thread.CurrentThread.ManagedThreadId);

        this.client = new PrismClient(new AppSettings {
            AppIdRealtime = "85e244b3-7dca-4b18-a818-f030e0a412cc",
            AppVersion = Application.version,
            UseNameServer = false,
            Server = "127.0.0.1",
            Port = 5055,
            Protocol = ConnectionProtocol.Udp,
        }).AddTo(this);

        UniTask.Create(async () => {
            Debug.LogFormat("UniTask.Create: {0}", Thread.CurrentThread.ManagedThreadId);

            await UniTask.SwitchToThreadPool();

            Debug.LogFormat("SwitchToThreadPool: {0}", Thread.CurrentThread.ManagedThreadId);

            await this.client.Run(async (client, token) => {
                Debug.LogFormat("Run: {0}", Thread.CurrentThread.ManagedThreadId);

                client.OnDisconnected.ObserveOnMainThread().Subscribe(cause => {
                    Debug.LogFormat("OnDisconnected [{0}]: {1}", Thread.CurrentThread.ManagedThreadId, cause);
                    this.progressLabel.SetActive(false);
                    this.controlPanel.SetActive(true);
                }).AddTo(token);

                // PhotonNetwork.ConnectUsingSettings();
                if (!client.Connect()) {
                    Debug.LogError("Connect Error");
                    return;
                }
                await client.OnConnectedToMaster.ToUniTask(true, token);
                Debug.LogFormat("OnConnectedToMaster [{0}]", Thread.CurrentThread.ManagedThreadId);

                // PhotonNetwork.JoinRandomRoom();
                var joinRoomTask = client.OnJoinedRoom.ToUniTask(true, token);
                var joinTask = UniTask.WhenAny(
                    joinRoomTask,
                    client.OnJoinRandomFailed.ToUniTask(true, token)
                );
                if (!client.OpJoinRandomRoom(new OpJoinRandomRoomParams {
                    MatchingType = MatchmakingMode.FillRoom,
                })) {
                    Debug.LogError("OpJoinRandomRoom Error");
                    return;
                }

                var joinResult = await joinTask;

                if (joinResult.winArgumentIndex == 1) {
                    var (returnCode, message) = joinResult.result2;
                    Debug.LogFormat("OnJoinRandomFailed [{0}]: {1} {2}", Thread.CurrentThread.ManagedThreadId, returnCode, message);

                    // PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
                    var createTask = UniTask.WhenAny(
                        joinRoomTask,
                        client.OnCreateRoomFailed.ToUniTask(true, token)
                    );
                    if (!client.OpCreateRoom(new EnterRoomParams {
                        RoomName = null,
                        RoomOptions = new RoomOptions { MaxPlayers = maxPlayersPerRoom },
                        Lobby = client.InLobby ? client.CurrentLobby : null,
                        ExpectedUsers = null
                    })) {
                        Debug.LogError("OpCreateRoom Error");
                        return;
                    }

                    var createResult = await createTask;

                    if (createResult.winArgumentIndex == 1) {
                        (returnCode, message) = createResult.result2;
                        Debug.LogErrorFormat("OnCreateRoomFailed [0]: {1} {2}", Thread.CurrentThread.ManagedThreadId, returnCode, message);
                        return;
                    } else if (createResult.winArgumentIndex != 0) {
                        Debug.LogError("createResult.winArgumentIndex Error");
                        return;
                    }
                } else if (joinResult.winArgumentIndex != 0) {
                    Debug.LogError("joinResult.winArgumentIndex Error");
                    return;
                }

                Debug.LogFormat("OnJoinedRoom [{0}]", Thread.CurrentThread.ManagedThreadId);

                // PhotonNetwork.LoadLevel("Room for 1");
            });


            Debug.LogFormat("ThreadId: {0}", Thread.CurrentThread.ManagedThreadId);
        }).Forget();
    }
}
