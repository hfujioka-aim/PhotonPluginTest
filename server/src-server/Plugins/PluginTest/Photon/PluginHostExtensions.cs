using System.Collections.Generic;

namespace Photon.Hive.Plugin
{
    internal static class PluginHostExtensions
    {
        public static void RaiseEvent(
            this IPluginHost host,
            byte eventCode, object eventData,
            byte receiverGroup = ReciverGroup.All,
            int senderActorNumber = 0,
            byte cachingOption = CacheOperations.DoNotCache,
            byte interestGroup = 0,
            SendParameters sendParams = default
        ) {
            var parameters = new Dictionary<byte, object> {
                { 245, eventData },
                { 254, senderActorNumber }
            };
            host.BroadcastEvent(receiverGroup, senderActorNumber, interestGroup, eventCode, parameters, cachingOption, sendParams);
        }

        public static void RaiseEvent(
            this IPluginHost host,
            byte eventCode, object eventData, IList<int> targetActorsNumbers,
            int senderActorNumber = 0,
            byte cachingOption = CacheOperations.DoNotCache,
            SendParameters sendParams = default
        ) {
            var parameters = new Dictionary<byte, object> {
                { 245, eventData },
                { 254, senderActorNumber }
            };
            host.BroadcastEvent(targetActorsNumbers, senderActorNumber, eventCode, parameters, cachingOption, sendParams);
        }
    }
}
