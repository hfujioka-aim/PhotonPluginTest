using System;
using System.Collections.Generic;

using static MasterData.PhotonAnimatorView;

namespace Photon.Pun
{
    internal class PhotonAnimatorView: IPhotonViewComponent
    {
        public MasterData.PhotonAnimatorView Master { get; }

        public PhotonAnimatorView(MasterData.PhotonAnimatorView master)
        {
            this.Master = master;
        }

        public byte[] SynchronizationTypeState { get; set; }

        public bool Deserialize(object[] data, ref int it)
        {
            var i = it;

            data.TryRead(out byte[] states, ref i);
            this.SynchronizationTypeState = states;

            if (!data.TryRead(out int sampleCount, ref i)) {
                return false;
            }

            if (!data.TryRead(out int objectPerSample, ref i)) {
                return false;
            }

            var len = sampleCount * objectPerSample;
            var objects = data.AsSpan(i, len);
            i += len;

            var layerWeight = new List<float>();
            foreach (var sl in this.Master.SynchronizeLayers) {
                if (sl.SynchronizeType == SynchronizeType.Discrete) {
                    if (!data.TryRead(out float weight, ref i)) {
                        return false;
                    }
                    layerWeight.Add(weight);
                }
            }

            foreach (var sp in this.Master.SynchronizeParameters) {
                if (sp.SynchronizeType == SynchronizeType.Discrete) {
                    switch (sp.Type) {
                        case ParameterType.Bool: {
                            if (!data.TryRead(out bool _, ref i)) {
                                return false;
                            }
                            break;
                        }
                        case ParameterType.Float: {
                            if (!data.TryRead(out float _, ref i)) {
                                return false;
                            }
                            break;
                        }
                        case ParameterType.Int: {
                            if (!data.TryRead(out int _, ref i)) {
                                return false;
                            }
                            break;
                        }
                        case ParameterType.Trigger: {
                            if (!data.TryRead(out bool _, ref i)) {
                                return false;
                            }
                            break;
                        }
                    }
                }
            }

            it = i;
            return true;
        }
    }
}
