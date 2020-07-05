using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
[RequireComponent(typeof(PhotonView), typeof(CameraWork))]
[RequireComponent(typeof(Renderer), typeof(CharacterController))]
public class PlayerManager: MonoBehaviourPunCallbacks, IPunObservable
{
    #region Private Fields

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    [Tooltip("The Beams GameObject to control")]
    [SerializeField]
    private GameObject beams;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    private GameObject playerUiPrefab;

    //True, when the user is firing
    bool IsFiring;

    #endregion

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (this.photonView.IsMine) {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }

        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        if (this.beams == null) {
            Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
        } else {
            this.beams.SetActive(false);
        }
    }

    /// <summary>
    /// 初期化の際にUnityによりGameObjectに呼び出されるMonoBehaviourメソッド
    /// </summary>
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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= this.CalledOnLevelWasLoaded;
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {
        if (this.photonView.IsMine) {
            this.ProcessInputs();
        }

        if (this.Health <= 0f) {
            GameManager.Instance.LeaveRoom();
        }

        // trigger Beams active state
        if (this.beams != null && this.IsFiring != this.beams.activeInHierarchy) {
            this.beams.SetActive(this.IsFiring);
        }
    }

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a beam
    /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
    /// One could move the collider further away to prevent this or check if the beam belongs to the player.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!this.photonView.IsMine) {
            return;
        }

        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam")) {
            return;
        }

        this.Health -= 0.1f;
    }

    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are touching the player
    /// </summary>
    /// <param name="other">Other.</param>
    void OnTriggerStay(Collider other)
    {
        // we dont' do anything if we are not the local player.
        if (!this.photonView.IsMine) {
            return;
        }

        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam")) {
            return;
        }

        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        this.Health -= 0.1f * Time.deltaTime;
    }

    void CalledOnLevelWasLoaded(Scene scene, LoadSceneMode loadingMode)
    {
        var _uiGo = Instantiate(this.playerUiPrefab);
        _uiGo.SendMessage(nameof(PlayerUI.SetTarget), this, SendMessageOptions.RequireReceiver);

        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(this.transform.position, -Vector3.up, 5f)) {
            this.transform.position = new Vector3(0f, 5f, 0f);
        }
    }

    #endregion

    #region Custom

    /// <summary>
    /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
    /// </summary>
    void ProcessInputs()
    {
        if (Input.GetButtonDown("Fire1")) {
            if (!this.IsFiring) {
                this.IsFiring = true;
            }
        }

        if (Input.GetButtonUp("Fire1")) {
            if (this.IsFiring) {
                this.IsFiring = false;
            }
        }
    }

    #endregion

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            //このプレイヤーを所有しています。データをほかのものに送ります。
            stream.SendNext(this.IsFiring);
            stream.SendNext(this.Health);
        } else {
            // ネットワークプレイヤー。データ受信
            this.IsFiring = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}