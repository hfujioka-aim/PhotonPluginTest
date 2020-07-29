using Photon.Pun;

using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerAnimatorManager: MonoBehaviourPun
{
    [SerializeField]
    private float directionDampTime = 0.25f;

    #region MonoBehaviour Callbacks

    private Animator animator;
    private PhotonTransformViewClassic tv;

    void Awake()
    {
        this.animator = this.GetComponent<Animator>();
        this.tv = this.GetComponent<PhotonTransformViewClassic>();
        if (!this.animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
    }

    private Vector3 NetworkPosition { get; set; }
    private float Distance { get; set; }
    private Quaternion NetworkRotation { get; set; }
    private float Angle { get; set; }

    private void OnEvent(ExitGames.Client.Photon.EventData ev)
    {
        if (ev.Code != 1) {
            return;
        }
        var data = (object[])ev.CustomData;
        if (data[0] is Vector3 pos) {
            this.NetworkPosition = pos;
        } else {
            this.NetworkPosition = this.transform.position;
        }

        if (data[1] is Quaternion rot) {
            this.NetworkRotation = rot;
        } else {
            this.NetworkRotation = this.transform.rotation;
        }
        this.Distance = Vector3.Distance(this.transform.position, this.NetworkPosition);
        this.Angle = Quaternion.Angle(this.transform.rotation, this.NetworkRotation);
        Debug.Log($"Distance: {Distance}");
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += this.OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= this.OnEvent;
    }

    void Update()
    {
        if (!this.photonView.IsMine && PhotonNetwork.IsConnected) {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (v == 0 && h == 0) {
            Debug.Log($"apply: {Vector3.Distance(this.NetworkPosition, this.transform.position)}");
            this.transform.position = this.NetworkPosition; // Vector3.MoveTowards(this.transform.position, this.NetworkPosition, this.Distance * (1.0f / PhotonNetwork.SerializationRate));
            // this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, this.NetworkRotation, this.Angle * (1.0f / PhotonNetwork.SerializationRate));
        } else {
            
        }

#if false
        if (!this.animator) {
            return;
        }

        // deal with Jumping
        AnimatorStateInfo stateInfo = this.animator.GetCurrentAnimatorStateInfo(0);

        // only allow jumping if we are running.
        if (stateInfo.IsName("Base Layer.Run")) {
            // When using trigger parameter
            if (Input.GetButtonDown("Fire2")) {
                this.animator.SetTrigger("Jump");
            }
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (v < 0) {
            v = 0;
        }

        this.animator.SetFloat("Speed", h * h + v * v);
        this.animator.SetFloat("Direction", h, this.directionDampTime, Time.deltaTime);

        if (this.tv) {
            this.tv.SetSynchronizedValues(this.animator.velocity, this.animator.angularVelocity.magnitude);
        }
#endif
    }

    #endregion
}
