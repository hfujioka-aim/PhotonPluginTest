using System;
using System.Linq.Expressions;

using Cysharp.Threading.Tasks;

using ExitGames.Client.Photon;

namespace Prism
{
    public abstract class EventHandler
    {
        public EventHandler() { }

        protected bool Register<T>(Func<T, UniTask> func)
        {
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EventCodeAttribute: Attribute
    {
        public EventCode Code { get; }

        public EventCodeAttribute(EventCode code)
        {
            this.Code = code;
        }
    }

    public enum EventCode: byte
    {
        AAAA = 1,
        BBBB,
        End = 200,
    }


    [EventCode(EventCode.AAAA)]
    public class HogeHandler: EventHandler
    {
        public HogeHandler()
        {
            this.Register<int>(this.OnEventAsync);
        }

        public async UniTask OnEventAsync(int arg)
        {
        }
    }
}
