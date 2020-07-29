using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

using MasterData;

using Photon.Hive.Plugin;

namespace PluginTest
{
    public class PluginFactory: IPluginFactory
    {
        public static string DllPath { get; }

        public static string BasePath { get; }

        public static Lazy<MasterHolder> Master { get; }

        static PluginFactory()
        {
            DllPath = Assembly.GetExecutingAssembly().Location;
            BasePath = Path.GetDirectoryName(DllPath);

            Master = new Lazy<MasterHolder>(() => {
                return MasterHolder.Load(Path.Combine(BasePath, "MasterData"));
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IGamePlugin Create(IPluginHost host, string pluginName, Dictionary<string, string> config, out string errorMsg)
        {
            host.LogInfo($"Create Plugin: {pluginName}");

            var plugin = new Plugin(Master.Value);
            if (plugin.SetupInstance(host, config, out errorMsg)) {
                return plugin;
            }
            return null;
        }
    }
}
