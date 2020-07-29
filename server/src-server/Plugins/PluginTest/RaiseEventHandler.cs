using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginTest
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal class EventCodeAttribute: Attribute
    {

    }

    internal class RaiseEventHandler
    {
        public Plugin Plugin { get; }

        public RaiseEventHandler(Plugin plugin)
        {
            this.Plugin = plugin;
        }
    }
}
