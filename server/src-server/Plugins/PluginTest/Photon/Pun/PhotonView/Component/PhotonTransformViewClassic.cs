using System;
using System.Numerics;

using MasterData;

namespace Photon.Pun
{
    public class PhotonTransformViewClassic: IPhotonViewComponent
    {
        public MasterData.PhotonTransformViewClassic Master { get; }

        public PhotonTransformViewClassic(MasterData.PhotonTransformViewClassic master)
        {
            this.Master = master;
        }

        public Vector3 Position { get; set; }

        public Vector3 Speed { get; set; }

        public float TurnSpeed { get; set; }

        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            if (this.Master.PositionModel.SynchronizeEnabled) {
                if (!data.TryRead(out Vector3 position, ref i)) {
                    return false;
                }
                this.Position = position;

                if (this.Master.PositionModel.ExtrapolateOption == PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues
                    || this.Master.PositionModel.InterpolateOption == PhotonTransformViewPositionModel.InterpolateOptions.SynchronizeValues
                ) {
                    if (!data.TryRead(out Vector3 speed, ref i)) {
                        return false;
                    }
                    this.Speed = speed;

                    if (!data.TryRead(out float turnSpeed, ref i)) {
                        return false;
                    }
                    this.TurnSpeed = turnSpeed;
                }
            }

            if (this.Master.RotationModel.SynchronizeEnabled) {
                if (!data.TryRead(out Quaternion rotation, ref i)) {
                    return false;
                }
                this.Rotation = rotation;
            }

            if (this.Master.ScaleModel.SynchronizeEnabled) {
                if (!data.TryRead(out Vector3 scale, ref i)) {
                    return false;
                }
                this.Scale = scale;
            }

            it = i;
            return true;
        }
    }
}
