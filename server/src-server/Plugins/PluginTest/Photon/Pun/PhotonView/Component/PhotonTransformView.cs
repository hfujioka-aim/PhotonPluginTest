using System;
using System.Numerics;

namespace Photon.Pun
{
    public class PhotonTransformView: IPhotonViewComponent
    {
        public MasterData.PhotonTransformView Master { get; }

        public PhotonTransformView(MasterData.PhotonTransformView master)
        {
            this.Master = master;
        }

        public Vector3 Position { get; set; }

        public Vector3 Direction { get; set; }
        
        public Quaternion Rotation { get; set; }

        public Vector3 Scale { get; set; }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            if (this.Master.SynchronizePosition) {
                if (!data.TryRead(out Vector3 position, ref i)) {
                    return false;
                }
                this.Position = position;

                if (!data.TryRead(out Vector3 direction, ref i)) {
                    return false;
                }
                this.Direction = direction;
            }
            
            if (this.Master.SynchronizeRotation) {
                if (!data.TryRead(out Quaternion rotation, ref i)) {
                    return false;
                }
                this.Rotation = rotation;
            }

            if (this.Master.SynchronizeScale) {
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
