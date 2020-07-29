using System;
using System.Numerics;

namespace Photon.Pun
{
    internal class PhotonTransformViewEx: IPhotonViewComponent
    {
        public MasterData.PhotonTransformViewEx Master { get; }

        public PhotonTransformViewEx(MasterData.PhotonTransformViewEx master)
        {
            this.Master = master;
        }

        public Vector3 Position { get; set; }

        public Vector3 DiffPosition { get; set; }

        public float Angle { get; set; }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            if (!data.TryRead(out Vector3 position, ref i)) {
                return false;
            }
            this.Position = position;

            if (!data.TryRead(out Vector3 diffPosition, ref i)) {
                return false;
            }
            this.DiffPosition = diffPosition;

            if (!data.TryRead(out float angle, ref i)) {
                return false;
            }
            this.Angle = angle;

            it = i;
            return true;
        }
    }
}
