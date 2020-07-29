using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using MasterData;

using Photon;
using Photon.Hive.Plugin;
using Photon.Pun;

namespace System
{
    public static class SerializerHelper
    {
        public static bool TryRead<T>(this object[] data, out T dest, ref int it)
        {
            if (it < data.Length && data[it] is T tmp) {
                dest = tmp;
                it++;
                return true;
            }

            dest = default;
            return false;
        }
    }
}

namespace PluginTest
{
    public class Plugin: PluginBase
    {
        public override string Name => nameof(Plugin);

        internal MasterHolder Master { get; }

        internal PhotonViewHolder PrefabHolder { get; }

        public Plugin(MasterHolder master)
        {
            this.Master = master;
            this.PrefabHolder = new PhotonViewHolder(master.PhotonView);
        }

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            var result = true;
            result &= host.RegisterPUNTypes();
            result &= base.SetupInstance(host, config, out errorMsg);
            return result;
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            this.PluginHost.LogInfo($"OnCreateGame {info.Request.GameId} by user {info.UserId}");
            info.Continue();
        }

        public override void BeforeJoin(IBeforeJoinGameCallInfo info)
        {
            base.BeforeJoin(info);
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);
        }

        // PhotonNetwork.OnEvent
        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            var tickCount = Environment.TickCount;
#if true
            switch (info.Request.EvCode) {
                case PunEvent.Instantiation: {
                    var data = new InstantiationData();
                    if (!data.Deserialize(info.Request)) {
                        break;
                    }
                    this.PrefabHolder.Instantiate(info.ActorNr, data);
                    break;
                }

                case PunEvent.Destroy: {
                    if (!(info.Request.Data is Hashtable data)) {
                        break;
                    }
                    if (!data.TryGetValue((byte)0, out int viewId)) {
                        break;
                    }
                    this.PrefabHolder.Destroy(viewId);
                    break;
                }

                case PunEvent.DestroyPlayer:
                case EventCode.Leave: // ???
                    // TODO
                    break;

                case PunEvent.RPC: {
                    if (!(info.Request.Data is Hashtable data)) {
                        break;
                    }

                    if (!data.TryGetValue((byte)0, out int viewId)) {
                        break;
                    }
                    var prefix = data.TryGetValue<short>((byte)1) ?? 0;

                    if (!data.TryGetValue((byte)2, out int timeStamp)) {
                        break;
                    }

                    var idx = data.TryGetValue<byte>((byte)5);
                    if (!idx.HasValue) {
                        if (!data.TryGetValue((byte)3, out string methodName)) {
                            break;
                        }
                    }

                    var args = data.TryGetValue<object[]>((byte)4) ?? null;

                    // skillRPCAsync
                    if (idx != 6) {
                        break;
                    }

                    var i = 0;
                    if (!args.TryRead(out byte beam, ref i)) {
                        break;
                    }
                    if (!args.TryRead(out Vector2 pos_xz, ref i)) {
                        break;
                    }
                    if (!args.TryRead(out float angle_y, ref i)) {
                        break;
                    }

                    if (!this.Master.Skill.TryGetValue(beam, out var skill)) {
                        break;
                    }

                    this.PluginHost.CreateOneTimeTimer(() => {
                        var hitTarget = this.PrefabHolder.Where(e => e.Key != viewId)
                            .Where(e => {
                                var target = e.Value.History.Last().Components.OfType<Photon.Pun.PhotonTransformViewEx>().First();
                                return Collider.IsHit(skill, pos_xz, angle_y, new Vector2(target.Position.X, target.Position.Z));
                            });

                        foreach (var target in hitTarget) {
                            var table = new Hashtable {
                                { (byte)0, target.Key },
                                { (byte)2, Environment.TickCount },
                                { (byte)5, (byte)5 },
                            };
                            this.PluginHost.RaiseEvent(PunEvent.RPC, table, sendParams: new SendParameters { Unreliable = false });
                        }
                    }, skill.DelayTime);

                    break;
                }

                case PunEvent.SendSerialize:
                case PunEvent.SendSerializeReliable: {
                    var data = new PhotonSerializeViewData(this.PrefabHolder);
                    if (!data.Deserialize(info.Request)) {
                        this.PluginHost.LogError("Deserialize ERROR");
                        break;
                    }
                    this.PluginHost.LogInfo($"TIME: SV {tickCount}, PKT {data.ServerTimestamp}");

                    foreach (var e in data.PhotonViews) {
                        if (!this.PrefabHolder.TryGetValue(e.ViewID, out var value)) {
                            continue;
                        }
                        value.History.Add(e);
                    }

                    break;
                }

                case PunEvent.OwnershipTransfer: {
                    if (!(info.Request.Data is int[] data)) {
                        break;
                    }
                    var viewID = data[0];
                    var playerID = data[1];
                    break;
                }
            }
#endif
            base.OnRaiseEvent(info);
        }

        public override void BeforeSetProperties(IBeforeSetPropertiesCallInfo info)
        {
            base.BeforeSetProperties(info);
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            base.OnSetProperties(info);
        }

        public override void BeforeCloseGame(IBeforeCloseGameCallInfo info)
        {
            base.BeforeCloseGame(info);
        }

        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            base.OnCloseGame(info);
        }

        public override bool OnUnknownType(Type type, ref object value)
        {
            return base.OnUnknownType(type, ref value);
        }
    }
}
