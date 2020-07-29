using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using MasterData;

namespace Photon.Pun
{
    public interface IPhotonViewComponent
    {
        bool Deserialize(object[] data, ref int it);
    }

    public class PhotonView
    {
        public MasterData.PhotonView Master { get; }

        public int ViewID { get; set; }

        public IReadOnlyList<IPhotonViewComponent> Components { get; }

        public PhotonView(MasterData.PhotonView master, int viewId)
        {
            this.Master = master;
            this.ViewID = viewId;
            this.Components = master.Components
                .Select(MakeComponent)
                .ToArray();
        }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            if (!data.TryRead(out bool isCompressed, ref i)) {
                return false;
            }

            if (!isCompressed) {
                if (data[i++] != null) {
                    return false;
                }

                foreach (var c in this.Components) {
                    if (!c.Deserialize(data, ref i)) {
                        return false;
                    }
                }
            }

            it = i;
            return true;
        }

        // TODO: ExpressionTree
        public static IPhotonViewComponent MakeComponent(PhotonViewComponent c)
        {
            var type = Type.GetType($"{typeof(IPhotonViewComponent).Namespace}.{c.TypeName}");
            if (type == null || !typeof(IPhotonViewComponent).IsAssignableFrom(type)) {
                return null;
            }

            return (IPhotonViewComponent)Activator.CreateInstance(type, c);
        }
    }
}
