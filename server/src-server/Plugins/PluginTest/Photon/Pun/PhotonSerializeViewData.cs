using System;
using System.Collections.Generic;

using Photon.Hive.Plugin;

namespace Photon.Pun
{
    internal class PhotonSerializeViewData
    {
        public PhotonViewHolder Holder { get; }

        public int ServerTimestamp { get; set; }

        public int CurrentLevelPrefix { get; set; }

        public IReadOnlyList<PhotonView> PhotonViews { get; private set; }

        public PhotonSerializeViewData(PhotonViewHolder holder)
        {
            this.Holder = holder;
        }

        public bool Deserialize(IRaiseEventRequest req)
        {
            if (!(req.Data is object[] data)) {
                return false;
            }

            var i = 0;

            if (!data.TryRead(out int time, ref i)) {
                return false;
            }
            this.ServerTimestamp = time;

            if (data[i] is null) {
                i++;
                this.CurrentLevelPrefix = 0;
            } else if (data.TryRead(out byte prefix, ref i)) {
                this.CurrentLevelPrefix = prefix;
            } else {
                return false;
            }

            var list = new List<PhotonView>(data.Length);
            this.PhotonViews = list;
            while (i < data.Length) {
                if (!data.TryRead(out object[] pvData, ref i)) {
                    continue;
                }

                var j = 0;
                if (!pvData.TryRead(out int viewId, ref j)) {
                    continue;
                }

                if (!this.Holder.TryGetValue(viewId, out var prefab)) {
                    continue;
                }

                var view = new PhotonView(prefab.Master, viewId);
                list.Add(view);
                if (!view.Deserialize(pvData, ref j)) {
                    continue;
                }
            }

            return true;
        }

    }
}
