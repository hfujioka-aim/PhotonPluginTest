using System;
using System.Threading;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

using Photon.Realtime;

using UniRx;

using UnityEngine;

namespace Prism
{

    public sealed class PrismClient: IDisposable
    {
        public PrismLoadBalancingClient Client { get; }

        public AppSettings Settings => this.Client.Settings;

        private PrismThread thread { get; }

        public PrismClient(AppSettings settings)
        {
            this.Client = new PrismLoadBalancingClient(settings);
            this.thread = new PrismThread(this.Client);

#if false
            //UniTask.Run
            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.PreUpdate).ForEachAsync(_ => {
                this.Client.DispatchIncomingCommands();
            }, cts.Token).Forget(Debug.LogException);

            UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.LastPostLateUpdate).ForEachAsync(_ => {
                var peer = this.Client.LoadBalancingPeer;
                var lastSendTime = Math.Max(peer.LastSendOutgoingTime, peer.LastSendAckTime);
                if (peer.ConnectionTime - lastSendTime >= 50) {
                    this.Client.SendOutgoingCommands();
                }
            }, cts.Token).Forget(Debug.LogException);
#endif

            // TODO:
            // Application.focusChanged += application_focusChanged;
        }

        public Task Run(Action<PrismLoadBalancingClient> func)
            => this.thread.TaskFactory.StartNew(this.runInvoke, func);

        public Task<T> Run<T>(Func<PrismLoadBalancingClient, T> func)
            => this.thread.TaskFactory.StartNew(this.runInvoke<T>, func);

        public UniTask Run(Func<PrismLoadBalancingClient, UniTask> func)
            => this.thread.TaskFactory.StartNew(this.runInvoke<UniTask>, func).Unwrap();

        public UniTask<T> Run<T>(Func<PrismLoadBalancingClient, UniTask<T>> func)
            => this.thread.TaskFactory.StartNew(this.runInvoke<UniTask<T>>, func).Unwrap();

        public Task Run(Action<PrismLoadBalancingClient, CancellationToken> func)
            => this.thread.TaskFactory.StartNew(this.runInvokeCancel, func);

        public Task<T> Run<T>(Func<PrismLoadBalancingClient, CancellationToken, T> func)
            => this.thread.TaskFactory.StartNew(this.runInvokeCancel<T>, func);

        public async UniTask Run(Func<PrismLoadBalancingClient, CancellationToken, UniTask> func)
        {
            var a = UniTask.ReturnToMainThread();
            await UniTask.SwitchToSynchronizationContext(this.thread.SC);
            await this.runInvokeCancelAsync(func);
            await a.DisposeAsync();
        }
       //     => this.thread.TaskFactory.StartNew(this.runInvokeCancelAsync, func).Unwrap();

        public UniTask<T> Run<T>(Func<PrismLoadBalancingClient, CancellationToken, UniTask<T>> func)
            => this.thread.TaskFactory.StartNew(this.runInvokeCancelAsync<T>, func).Unwrap();

        private void runInvoke(object obj)
        {
            var func = (Action<PrismLoadBalancingClient>)obj;
            func(this.Client);
        }

        private T runInvoke<T>(object obj)
        {
            var func = (Func<PrismLoadBalancingClient, T>)obj;
            return func(this.Client);
        }

        private void runInvokeCancel(object obj)
        {
            var func = (Action<PrismLoadBalancingClient, CancellationToken>)obj;
            using (var cts = new CancellationTokenSource())
            using (new CancellationDisposable(cts))
            using (this.Client.OnDisconnected.Subscribe(_ => cts.Cancel())) {
                func(this.Client, cts.Token);
            }
        }

        private T runInvokeCancel<T>(object obj)
        {
            var func = (Func<PrismLoadBalancingClient, CancellationToken, T>)obj;
            using (var cts = new CancellationTokenSource())
            using (new CancellationDisposable(cts))
            using (this.Client.OnDisconnected.Subscribe(_ => cts.Cancel())) {
                return func(this.Client, cts.Token);
            }
        }

        private async UniTask runInvokeCancelAsync(object obj)
        {
            var func = (Func<PrismLoadBalancingClient, CancellationToken, UniTask>)obj;
            using (var cts = new CancellationTokenSource())
            using (new CancellationDisposable(cts))
            using (this.Client.OnDisconnected.Subscribe(_ => cts.Cancel())) {
                await func(this.Client, cts.Token);
            }
        }

        private async UniTask<T> runInvokeCancelAsync<T>(object obj)
        {
            var func = (Func<PrismLoadBalancingClient, CancellationToken, UniTask<T>>)obj;
            using (var cts = new CancellationTokenSource())
            using (new CancellationDisposable(cts))
            using (this.Client.OnDisconnected.Subscribe(_ => cts.Cancel())) {
                return await func(this.Client, cts.Token);
            }
        }

        public void Dispose()
        {
            this.thread.Dispose();
            this.Client.Dispose();
        }
    }
}
