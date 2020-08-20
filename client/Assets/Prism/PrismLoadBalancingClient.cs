using System;
using System.Collections.Generic;

using ExitGames.Client.Photon;

using Photon.Realtime;

using UniRx;

namespace Prism
{
    public class PrismLoadBalancingClient:
        PrismLoadBalancingClientBase,
        IConnectionCallbacks, IInRoomCallbacks, ILobbyCallbacks,
        IMatchmakingCallbacks, IWebRpcCallback, IErrorInfoCallback
    {
        public new IObservable<(ClientState oldState, ClientState newState)> StateChanged { get; }
        public IObservable<(ClientState oldState, ClientState newState)> OnStateChanged => this.StateChanged;

        public new IObservable<StatusCode> StatusChanged { get; }
        public new IObservable<StatusCode> OnStatusChanged => this.StatusChanged;

        public new IObservable<EventData> EventReceived { get; }
        public new IObservable<EventData> OnEvent => this.EventReceived;

        public new IObservable<OperationResponse> OpResponseReceived { get; }
        public new IObservable<OperationResponse> OnOperationResponse => this.OpResponseReceived;

        public PrismLoadBalancingClient(AppSettings settings) : base(settings)
        {
            this.StateChanged = Observable.FromEvent<Action<ClientState, ClientState>, (ClientState, ClientState)>(
                h => (a, b) => h((a, b)),
                h => base.StateChanged += h,
                h => base.StateChanged -= h
            );

            this.StatusChanged = Observable.FromEvent<StatusCode>(
                h => base.StatusChanged += h,
                h => base.StatusChanged -= h
            );

            this.EventReceived = Observable.FromEvent<EventData>(
                h => base.EventReceived += h,
                h => base.EventReceived -= h
            );

            this.OpResponseReceived = Observable.FromEvent<OperationResponse>(
                h => base.OpResponseReceived += h,
                h => base.OpResponseReceived -= h
            );

            this.AddCallbackTarget(this);
        }

        #region IConnectionCallbacks
        public IObservable<Unit> OnConnected => this.onConnected;
        private Subject<Unit> onConnected { get; } = new Subject<Unit>();
        void IConnectionCallbacks.OnConnected()
            => this.onConnected.OnNext(Unit.Default);

        public IObservable<Unit> OnConnectedToMaster => this.onConnectedToMaster;
        private Subject<Unit> onConnectedToMaster { get; } = new Subject<Unit>();
        void IConnectionCallbacks.OnConnectedToMaster()
            => this.onConnectedToMaster.OnNext(Unit.Default);

        public IObservable<string> OnCustomAuthenticationFailed => this.onCustomAuthenticationFailed;
        private Subject<string> onCustomAuthenticationFailed { get; } = new Subject<string>();
        void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
            => this.onCustomAuthenticationFailed.OnNext(debugMessage);

        public IObservable<Dictionary<string, object>> OnCustomAuthenticationResponse => this.onCustomAuthenticationResponse;
        private Subject<Dictionary<string, object>> onCustomAuthenticationResponse { get; } = new Subject<Dictionary<string, object>>();
        void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
            => this.onCustomAuthenticationResponse.OnNext(data);

        public IObservable<DisconnectCause> OnDisconnected => this.onDisconnected;
        private Subject<DisconnectCause> onDisconnected { get; } = new Subject<DisconnectCause>();
        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
            => this.onDisconnected.OnNext(cause);

        public IObservable<RegionHandler> OnRegionListReceived => this.onRegionListReceived;
        private Subject<RegionHandler> onRegionListReceived { get; } = new Subject<RegionHandler>();
        void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
            => this.onRegionListReceived.OnNext(regionHandler);
        #endregion IConnectionCallbacks

        #region IInRoomCallbacks
        public IObservable<Player> OnMasterClientSwitched => this.onMasterClientSwitched;
        private Subject<Player> onMasterClientSwitched { get; } = new Subject<Player>();
        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
            => this.onMasterClientSwitched.OnNext(newMasterClient);

        public IObservable<Player> OnPlayerEnteredRoom => this.onPlayerEnteredRoom;
        private Subject<Player> onPlayerEnteredRoom { get; } = new Subject<Player>();
        void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
            => this.onPlayerEnteredRoom.OnNext(newPlayer);

        public IObservable<Player> OnPlayerLeftRoom => this.onPlayerLeftRoom;
        private Subject<Player> onPlayerLeftRoom { get; } = new Subject<Player>();
        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
            => this.onPlayerLeftRoom.OnNext(otherPlayer);

        public IObservable<(Player targetPlayer, Hashtable changedProps)> OnPlayerPropertiesUpdate => this.onPlayerPropertiesUpdate;
        private Subject<(Player, Hashtable)> onPlayerPropertiesUpdate { get; } = new Subject<(Player, Hashtable)>();
        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
            => this.onPlayerPropertiesUpdate.OnNext((targetPlayer, changedProps));

        public IObservable<Hashtable> OnRoomPropertiesUpdate => this.onRoomPropertiesUpdate;
        private Subject<Hashtable> onRoomPropertiesUpdate { get; } = new Subject<Hashtable>();
        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
            => this.onRoomPropertiesUpdate.OnNext(propertiesThatChanged);
        #endregion IInRoomCallbacks

        #region ILobbyCallbacks
        public IObservable<Unit> OnJoinedLobby => this.onJoinedLobby;
        private Subject<Unit> onJoinedLobby { get; } = new Subject<Unit>();
        void ILobbyCallbacks.OnJoinedLobby()
            => this.onJoinedLobby.OnNext(Unit.Default);

        public IObservable<Unit> OnLeftLobby => this.onLeftLobby;
        private Subject<Unit> onLeftLobby { get; } = new Subject<Unit>();
        void ILobbyCallbacks.OnLeftLobby()
            => this.onLeftLobby.OnNext(Unit.Default);

        public IObservable<Unit> OnRoomListUpdate => this.onRoomListUpdate;
        private Subject<Unit> onRoomListUpdate { get; } = new Subject<Unit>();
        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
            => this.onRoomListUpdate.OnNext(Unit.Default);

        public IObservable<Unit> OnLobbyStatisticsUpdate => this.onLobbyStatisticsUpdate;
        private Subject<Unit> onLobbyStatisticsUpdate { get; } = new Subject<Unit>();
        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
            => this.onLobbyStatisticsUpdate.OnNext(Unit.Default);
        #endregion ILobbyCallbacks

        #region IMatchmakingCallbacks
        public IObservable<List<FriendInfo>> OnFriendListUpdate => this.onFriendListUpdate;
        private Subject<List<FriendInfo>> onFriendListUpdate { get; } = new Subject<List<FriendInfo>>();
        void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
            => this.onFriendListUpdate.OnNext(friendList);

        public IObservable<Unit> OnCreatedRoom => this.onCreatedRoom;
        private Subject<Unit> onCreatedRoom { get; } = new Subject<Unit>();
        void IMatchmakingCallbacks.OnCreatedRoom()
            => this.onCreatedRoom.OnNext(Unit.Default);

        public IObservable<(short returnCode, string message)> OnCreateRoomFailed => this.onCreateRoomFailed;
        private Subject<(short, string)> onCreateRoomFailed { get; } = new Subject<(short, string)>();
        void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
            => this.onCreateRoomFailed.OnNext((returnCode, message));

        public IObservable<Unit> OnJoinedRoom => this.onJoinedRoom;
        private Subject<Unit> onJoinedRoom { get; } = new Subject<Unit>();
        void IMatchmakingCallbacks.OnJoinedRoom()
            => this.onJoinedRoom.OnNext(Unit.Default);

        public IObservable<(short returnCode, string message)> OnJoinRoomFailed => this.onJoinRoomFailed;
        private Subject<(short, string)> onJoinRoomFailed { get; } = new Subject<(short, string)>();
        void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
            => this.onJoinRoomFailed.OnNext((returnCode, message));

        public IObservable<(short returnCode, string message)> OnJoinRandomFailed => this.onJoinRandomFailed;
        private Subject<(short, string)> onJoinRandomFailed { get; } = new Subject<(short, string)>();
        void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
            => this.onJoinRandomFailed.OnNext((returnCode, message));

        public IObservable<Unit> OnLeftRoom => this.onLeftRoom;
        private Subject<Unit> onLeftRoom { get; } = new Subject<Unit>();
        void IMatchmakingCallbacks.OnLeftRoom()
            => this.onLeftRoom.OnNext(Unit.Default);
        #endregion IMatchmakingCallbacks

        #region IWebRpcCallback
        public IObservable<OperationResponse> OnWebRpcResponse => this.onWebRpcResponse;
        private Subject<OperationResponse> onWebRpcResponse { get; } = new Subject<OperationResponse>();
        void IWebRpcCallback.OnWebRpcResponse(OperationResponse response)
            => this.onWebRpcResponse.OnNext(response);
        #endregion IWebRpcCallback

        #region IErrorInfoCallback
        public IObservable<ErrorInfo> OnErrorInfo => this.onErrorInfo;
        private Subject<ErrorInfo> onErrorInfo { get; } = new Subject<ErrorInfo>();
        void IErrorInfoCallback.OnErrorInfo(ErrorInfo errorInfo)
            => this.onErrorInfo.OnNext(errorInfo);
        #endregion IErrorInfoCallback

        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue) {
                return;
            }
            base.Dispose(disposing);

            if (disposing) {
                this.RemoveCallbackTarget(this);
            }

            this.onConnected.Dispose();
            this.onConnectedToMaster.Dispose();
            this.onCustomAuthenticationFailed.Dispose();
            this.onCustomAuthenticationResponse.Dispose();
            this.onDisconnected.Dispose();
            this.onRegionListReceived.Dispose();
            this.onMasterClientSwitched.Dispose();
            this.onPlayerEnteredRoom.Dispose();
            this.onPlayerLeftRoom.Dispose();
            this.onPlayerPropertiesUpdate.Dispose();
            this.onRoomPropertiesUpdate.Dispose();
            this.onJoinedLobby.Dispose();
            this.onLeftLobby.Dispose();
            this.onRoomListUpdate.Dispose();
            this.onLobbyStatisticsUpdate.Dispose();
            this.onFriendListUpdate.Dispose();
            this.onCreatedRoom.Dispose();
            this.onCreateRoomFailed.Dispose();
            this.onJoinedRoom.Dispose();
            this.onJoinRoomFailed.Dispose();
            this.onJoinRandomFailed.Dispose();
            this.onLeftRoom.Dispose();
            this.onWebRpcResponse.Dispose();
            this.onErrorInfo.Dispose();
        }
    }
}
