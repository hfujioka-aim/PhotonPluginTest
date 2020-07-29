
using Photon.Pun;

using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PhotonTransformViewEx: MonoBehaviour, IPunObservable
{
    private new Transform transform;
    private PhotonView photonView;
    private CharacterController controller;

    private Vector3 recvPos;
    private Vector3 recvDiffPos;
    private Vector3 prevPos;
    private Vector3 estimatePos;
    private float speed; // [m/s]

    private float recvRot_y;
    private float rotSpeed; // [angle/s]

    public void Awake()
    {
        this.transform = base.transform;
        this.photonView = this.GetComponent<PhotonView>();
        this.controller = this.GetComponent<CharacterController>();
        this.recvPos = this.transform.position;
        this.prevPos = this.recvPos;
        this.estimatePos = this.recvPos;
        this.speed = 0;
        this.recvRot_y = this.transform.rotation.y;
        this.rotSpeed = 0;
    }

    public void Update()
    {
        if (this.photonView.IsMine) {
            return;
        }

        var pos = Vector3.MoveTowards(this.transform.position, this.estimatePos, this.speed);
        var rot = this.transform.rotation;
        rot = Quaternion.RotateTowards(rot, Quaternion.Euler(rot.x, this.recvRot_y, rot.z), this.rotSpeed);

        this.transform.SetPositionAndRotation(pos, rot);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            var pos = this.transform.position;
            stream.SendNext(pos);
            stream.SendNext(pos - this.prevPos);
            this.prevPos = pos;
            stream.SendNext(this.transform.eulerAngles.y);
        } else {
            this.recvPos = (Vector3)stream.ReceiveNext();
            this.recvDiffPos = (Vector3)stream.ReceiveNext();
            var lag = 0; // Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.estimatePos = this.recvPos + (this.recvDiffPos * PhotonNetwork.SerializationRate * lag);
            this.speed = Vector3.Distance(this.transform.position, this.estimatePos) * PhotonNetwork.SerializationRate * PhotonNetwork.SerializationRate;

            this.recvRot_y = (float)stream.ReceiveNext();
            var angle = this.transform.eulerAngles;
            var rot = Quaternion.Euler(angle.x, this.recvRot_y, angle.z);
            this.rotSpeed = Quaternion.Angle(this.transform.rotation, rot) * PhotonNetwork.SerializationRate * PhotonNetwork.SerializationRate;
        }
    }
}
