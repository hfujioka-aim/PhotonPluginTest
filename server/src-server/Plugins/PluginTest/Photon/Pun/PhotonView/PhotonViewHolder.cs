using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

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

        private readonly List<PhotonView> history = new List<PhotonView>();

        public IReadOnlyList<PhotonView> History => this.history;

        public void Add(PhotonView pv)
        {
            if (!this.isDash) {
                this.postDash = true;
            }
            this.history.Add(pv);
        }

        private bool isDash = false;
        private bool postDash = true;
        public bool IsDash {
            get => this.isDash;
            set {
                this.isDash = value;
                if (value) {
                    this.postDash = false;
                }
            }
        }

        public Vector2 DashDestPos { get; set; }

        public double GetCurrentAngle()
        {
            var transformView = this.History.LastOrDefault()?.Components?.OfType<PhotonTransformViewEx>().FirstOrDefault();
            if (transformView != null) {
                return transformView.Angle * Math.PI / 180;
            }
            var rot = this.InstantiationData.Rotation;
            return rot.Y * Math.Sin(Math.Acos(rot.W));
        }

        public Vector2 GetCurrentPosition()
        {
            if (!this.postDash) {
                return this.DashDestPos;
            }
            var pos = this.History.LastOrDefault()?.Components?.OfType<PhotonTransformViewEx>().FirstOrDefault()?.Position ?? this.InstantiationData.Position;
            return new Vector2(pos.X, pos.Z);
        }
    }
}
