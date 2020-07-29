using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;

using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView), typeof(CameraWork))]
[RequireComponent(typeof(Renderer), typeof(CharacterController))]
public class PlayerManager: MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    public bool ClientHit = false;

    [Tooltip("The Beams GameObject to control")]
    [SerializeField]
    private GameObject[] beams;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    private GameObject playerUiPrefab;

    bool IsFiring;

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    private CharacterController controller;

    public bool IsHit { get; private set; } = false;

    void Awake()
    {
        if (this.photonView.IsMine) {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }

        this.controller = this.GetComponent<CharacterController>();

        DontDestroyOnLoad(this.gameObject);

        foreach (var b in this.beams) {
            b.SetActive(false);
        }
    }

    void Start()
    {
        var _cameraWork = this.GetComponent<CameraWork>();
        if (_cameraWork != null) {
            if (this.photonView.IsMine) {
                _cameraWork.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        SceneManager.sceneLoaded += this.CalledOnLevelWasLoaded;

        if (this.playerUiPrefab != null) {
            var _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage(nameof(PlayerUI.SetTarget), this, SendMessageOptions.RequireReceiver);
        } else {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }
    }

    private CancellationTokenSource cts { get; set; }

    void OnEnable()
    {
        this.cts?.Dispose();
        this.cts = new CancellationTokenSource();
    }

    void OnDisable()
    {
        this.cts.Cancel();
    }

    void OnDestroy()
    {
        this.cts?.Dispose();
        this.cts = null;
        SceneManager.sceneLoaded -= this.CalledOnLevelWasLoaded;
    }

    void CalledOnLevelWasLoaded(Scene scene, LoadSceneMode loadingMode)
    {
        var _uiGo = Instantiate(this.playerUiPrefab);
        _uiGo.SendMessage(nameof(PlayerUI.SetTarget), this, SendMessageOptions.RequireReceiver);

        if (!Physics.Raycast(this.transform.position, -Vector3.up, 5f)) {
            this.transform.position = new Vector3(0f, 5f, 0f);
        }
    }

    void Update()
    {
        if (!this.photonView.IsMine && PhotonNetwork.IsConnected) {
            return;
        }

        if (!this.skillWait) {
            this.move();
            this.ProcessInputs();
        }

#if false
        if (this.Health <= 0f) {
            GameManager.Instance.LeaveRoom();
        }
#endif
    }

    private void move()
    {
        var dir = Vector3.zero;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (this.controller.isGrounded) {
            this.transform.Rotate(new Vector3(0, h * 1.2f, 0));
            dir = this.transform.forward * v * 5;
        }

        dir.y -= 10;
        this.controller.Move(dir * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!this.onTrigger(other)) {
            colliders.Remove(other);
        }

        // this.Health -= 0.1f;
    }

    void OnTriggerStay(Collider other)
    {
        if (!this.onTrigger(other)) {
            colliders.Remove(other);
        }

        // this.Health -= 0.1f * Time.deltaTime;
    }

    void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
    }

    private HashSet<Collider> colliders { get; } = new HashSet<Collider>();

    private bool onTrigger(Collider other)
    {
        if (!this.ClientHit || this.photonView.IsMine) {
            return false;
        }

        if (!other.CompareTag("Beam") || this.beams.Contains(other.gameObject)) {
            return false;
        }

        var beam = other.GetComponent<Beam>();
        if (!beam.IsActive) {
            return false;
        }
        if (!(beam?.owner?.GetComponent<PhotonView>()?.IsMine ?? false)) {
            return false;
        }

        if (!colliders.Add(other)) {
            return true;
        }

        this.photonView.RPC(nameof(skillHitRPCAsync), RpcTarget.All);
        return true;
    }

    void ProcessInputs()
    {
        this.CheckBeam(0, "Fire1");
        this.CheckBeam(1, "Fire2");
        this.CheckBeam(2, "Fire3");
    }

    private bool skillWait = false;

    void CheckBeam(byte idx, string buttonName)
    {
        if (Input.GetButtonDown(buttonName)) {
            this.useSkill(idx);
            // PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    private void useSkill(byte idx)
    {
        this.skillWait = true;
        var pos = this.transform.position;
        this.photonView.RPC(nameof(skillRPCAsync), RpcTarget.AllViaServer, idx, new Vector2(pos.x, pos.z), this.transform.eulerAngles.y);
    }

    [PunRPC]
    private async void skillRPCAsync(byte idx, Vector2 pos_xz, float angle_y, PhotonMessageInfo info)
    {
        var pos = this.transform.position;
        pos.x = pos_xz.x;
        pos.z = pos_xz.y;
        var angle = this.transform.eulerAngles;
        angle.y = angle_y;
        this.transform.SetPositionAndRotation(pos, Quaternion.Euler(angle));

        var beam = this.beams[idx];
        beam.SetActive(true);
        await Task.Delay(beam.GetComponent<Beam>().DelayTimeMs, this.cts.Token);
        beam.GetComponent<Beam>().SetActive(true);
        await Task.Delay(250, this.cts.Token);
        beam.SetActive(false);
        this.skillWait = false;
    }

    [PunRPC]
    private async void skillHitRPCAsync(PhotonMessageInfo info)
    {
        if (this.ClientHit && (info.Sender?.ActorNumber ?? 0) == 0) {
            return;
        } else if (!this.ClientHit && (info.Sender?.ActorNumber ?? 0) != 0) {
            return;
        }

        this.IsHit = true;
        await Task.Delay(1000, this.cts.Token);
        this.IsHit = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(this.IsFiring);
            stream.SendNext(this.Health);
        } else {
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }
}
