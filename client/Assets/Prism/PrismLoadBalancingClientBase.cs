using System;

using ExitGames.Client.Photon;

using MessagePack;

using Photon.Realtime;

namespace Prism
{

    public class PrismLoadBalancingClientBase:
        LoadBalancingClient, ILoadBalancingClient, IDisposable
    {
        public AppSettings Settings { get; }

        public PrismLoadBalancingClientBase(AppSettings settings)
            : base(settings.Protocol)
        {
            this.Settings = settings;
        }

        public new bool Connect()
            => this.ConnectUsingSettings(this.Settings);

        protected override Player CreatePlayer(string actorName, int actorNumber, bool isLocal, Hashtable actorProperties)
            => new PrismPlayer(actorName, actorNumber, isLocal, actorProperties);

        protected override Room CreateRoom(string roomName, RoomOptions opt)
            => new PrismRoom(roomName, opt);

        public bool DispatchIncomingCommands()
        {
            var isExec = false;
            while (this.LoadBalancingPeer?.DispatchIncomingCommands() ?? false) {
                isExec = true;
            }
            return isExec;
        }

        public bool SendOutgoingCommands()
        {
            var isExec = false;
            while (this.LoadBalancingPeer?.SendOutgoingCommands() ?? false) {
                isExec = true;
            }
            return isExec;
        }

        public override void OnEvent(EventData photonEvent)
        {
            base.OnEvent(photonEvent);
            // TODO:
            if (photonEvent.Code < 200 && photonEvent.CustomData is byte[] data) {
                switch (photonEvent.Code) {
                    case 100:
                        OnEventCode100(photonEvent, MessagePackSerializer.Deserialize<int>(data));
                        break;
                }

                // MessagePackSerializer.Deserialize<int>(data);
            }
        }

        public void OnEventCode100(EventData data, int arg)
        {

        }

        public override void OnOperationResponse(OperationResponse operationResponse)
        {
            base.OnOperationResponse(operationResponse);
        }

        public event Action<StatusCode> StatusChanged;
        public override void OnStatusChanged(StatusCode statusCode)
        {
            base.OnStatusChanged(statusCode);
            this.StatusChanged?.Invoke(statusCode);
        }

        #region IDisposable
        protected bool DisposedValue { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (this.DisposedValue) {
                return;
            }
            this.DisposedValue = true;

            if (disposing) {
                if (this.IsConnected) {
                    this.Disconnect();
                }
            }

            this.LoadBalancingPeer.StopThread();
        }

        ~PrismLoadBalancingClientBase()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion IDisposable
    }
}
