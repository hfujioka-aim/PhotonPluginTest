using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using MasterData;

namespace Photon.Pun
{
    internal class PhotonViewHolder: ReadOnlyDictionary<int, PhotonViewHistory>
    {
        public PhotonViewPrefabMaster Master { get; }

        public PhotonViewHolder(PhotonViewPrefabMaster master)
            : base(new Dictionary<int, PhotonViewHistory>())
        {
            this.Master = master;
        }

        public bool Instantiate(int actorNr, InstantiationData data)
        {
            if (string.IsNullOrEmpty(data.PrefabName)) {
                return false;
            }

            if (!this.Master.TryGetValue(data.PrefabName, out var prefab)) {
                return false;
            }

            this.Dictionary.Add(data.InstantiationID, new PhotonViewHistory {
                CreatorActorNr = actorNr,
                OwnerActorNr = actorNr,
                Master = prefab,
                InstantiationData = data,
            });

            return true;
        }

        public bool Destroy(int id)
        {
            return this.Dictionary.Remove(id);
        }
    }

    internal class PhotonViewHistory
    {
        public MasterData.PhotonView Master { get; set; }

        public InstantiationData InstantiationData { get; set; }

        public int ViewID => this.InstantiationData.InstantiationID;

        public int CreatorActorNr { get; set; }

        public int OwnerActorNr { get; set; }

        public List<PhotonView> History { get; } = new List<PhotonView>();
    }
}
