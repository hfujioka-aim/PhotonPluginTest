using System;

namespace Photon.Pun
{
    public class PlayerManager: IPhotonViewComponent
    {
        public MasterData.PlayerManager Master { get; }

        public PlayerManager(MasterData.PlayerManager master)
        {
            this.Master = master;
        }

        public bool IsFiring { get; set; }

        public float Health { get; set; }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            if (!data.TryRead(out bool isFiring, ref i)) {
                return false;
            }
            this.IsFiring = IsFiring;

            if (!data.TryRead(out float health, ref i)) {
                return false;
            }
            this.Health = health;

            it = i;
            return true;
        }
    }
}
