using Photon.Pun;
using Photon.Realtime;

using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
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

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
    }

    public void Start()
    {
        this.progressLabel.SetActive(false);
        this.controlPanel.SetActive(true);
    }

    public void Connect()
    {
        this.progressLabel.SetActive(true);
        this.controlPanel.SetActive(false);

        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.JoinRandomRoom();
        } else {
            PhotonNetwork.GameVersion = this.gameVersion;
            this.isConnecting = PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

        if (this.isConnecting) {
            PhotonNetwork.JoinRandomRoom();
            this.isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        this.progressLabel.SetActive(false);
        this.controlPanel.SetActive(true);
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            Debug.Log("We load the 'Room for 1' ");

            // #Critical
            // Load the Room Level.
            PhotonNetwork.LoadLevel("Room for 1");
        }
    }
}
