using Photon.Pun;

using UnityEngine;

[RequireComponent(typeof(PhotonView), typeof(Animator))]
public class PlayerAnimatorManager: MonoBehaviourPun
{
    [SerializeField]
    private float directionDampTime = 0.25f;

    #region MonoBehaviour Callbacks

    private Animator animator;

    void Start()
    {
        this.animator = this.GetComponent<Animator>();
        if (!this.animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
    }

    void Update()
    {
        if (!this.photonView.IsMine && PhotonNetwork.IsConnected) {
            return;
        }

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
    }

    #endregion
}
