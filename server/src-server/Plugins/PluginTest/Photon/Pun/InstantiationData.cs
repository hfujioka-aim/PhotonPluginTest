using System.Collections;
using System.Numerics;

using Photon.Hive.Plugin;

namespace Photon.Pun
{
    internal class InstantiationData
    {
        public string PrefabName { get; private set; }

        public int ServerTime { get; private set; }

        public int InstantiationID { get; private set; }

        public Vector3 Position { get; private set; }

        public Quaternion Rotation { get; private set; }

        public byte Group { get; private set; }

        public byte ObjLevelPrefix { get; private set; }

        public int[] ViewIDs { get; private set; }

        public object[] IncomingInstantiationData { get; private set; }

        public bool Deserialize(IRaiseEventRequest req)
        {
            if (!(req.Data is Hashtable data)) {
                return false;
            }

            if (!data.TryGetValue((byte)0, out string prefabName)) {
                return false;
            }
            this.PrefabName = prefabName;

            if (!data.TryGetValue((byte)6, out int serverTime)) {
                return false;
            }
            this.ServerTime = serverTime;

            if (!data.TryGetValue((byte)7, out int instantiationId)) {
                return false;
            }
            this.InstantiationID = instantiationId;

            this.Position = data.TryGetValue<Vector3>((byte)1) ?? Vector3.Zero;

            this.Rotation = data.TryGetValue<Quaternion>((byte)2) ?? Quaternion.Identity;

            this.Group = data.TryGetValue<byte>((byte)3) ?? 0;

            this.ObjLevelPrefix = data.TryGetValue<byte>((byte)8) ?? 0;

            this.ViewIDs =data.TryGetValue<int[]>((byte)4) ?? new int[] { instantiationId };

            this.IncomingInstantiationData = data.TryGetValue<object[]>((byte)5) ?? null;

            return true;
        }
    }
}
