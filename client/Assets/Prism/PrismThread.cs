using System;
using System.Threading;
using System.Threading.Tasks;

using Photon.Realtime;

namespace Prism
{
    internal interface ILoadBalancingClient
    {
        bool DispatchIncomingCommands();
        bool SendOutgoingCommands();
        bool IsConnected { get; }
        LoadBalancingPeer LoadBalancingPeer { get; }
        void Disconnect(DisconnectCause cause);
    }

    internal class PrismThread: IDisposable
    {
        private Thread thread { get; }

        private ILoadBalancingClient client { get; }

        public TaskFactory TaskFactory { get; private set; }

        public SynchronizationContext SC { get; private set; }

        public PrismThread(ILoadBalancingClient client, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.client = client;

            this.thread = new Thread(this.threadMain) {
                Name = nameof(PrismThread),
                IsBackground = true,
                Priority = priority,
            };

            using (var mre = new ManualResetEventSlim(false)) {
                this.thread.Start(mre);
                mre.Wait();
            }
        }

        private CancellationTokenSource threadStopCts { get; } = new CancellationTokenSource();

        private void threadMain(object arg)
        {
            ConcurrentQueueSynchronizationContext sc = null;
            try {
                sc = new ConcurrentQueueSynchronizationContext();
                this.SC = sc;
                SynchronizationContext.SetSynchronizationContext(sc);
                this.TaskFactory = new TaskFactory(this.threadStopCts.Token,
                    TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach,
                    TaskContinuationOptions.HideScheduler | TaskContinuationOptions.DenyChildAttach,
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
            } catch {
                SynchronizationContext.SetSynchronizationContext(null);
                sc?.Dispose();
                throw;
            } finally {
                Thread.MemoryBarrier();
                ((ManualResetEventSlim)arg).Set();
            }

            using (sc) {
                var stopToken = this.threadStopCts.Token;

                while (!stopToken.IsCancellationRequested || sc.HasPendingTasks()) {
                    try {
                        while (this.client.IsConnected || sc.HasPendingTasks()) {
                            var isExec = false;
                            isExec |= this.client.DispatchIncomingCommands();
                            isExec |= sc.DispatchAsyncTasks();

                            // 20FPS (=> 50ms)
                            var lastSendTimeSpan = this.client.LoadBalancingPeer.ConnectionTime - Math.Max(
                                this.client.LoadBalancingPeer.LastSendOutgoingTime,
                                this.client.LoadBalancingPeer.LastSendAckTime
                            );
                            if (lastSendTimeSpan > 50 || stopToken.IsCancellationRequested) {
                                isExec |= this.client.SendOutgoingCommands();
                            }

                            if ((isExec && !stopToken.IsCancellationRequested)
                                || (!isExec && stopToken.IsCancellationRequested)
                            ) {
                                Thread.Yield();
                            } else if (!isExec) {
                                sc.Wait(10, stopToken);
                            }
                        }
                        sc.Wait(stopToken);
                    } catch (OperationCanceledException) {
                    } catch {
                        this.client.Disconnect(DisconnectCause.Exception);
                    }
                }
            }
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposedValue) {
                return;
            }
            this.disposedValue = true;
            if (disposing) {
                if (this.client.IsConnected) {
                    this.TaskFactory.StartNew(() => {
                        this.client.Disconnect(DisconnectCause.DisconnectByClientLogic);
                    }).Wait();
                }
                this.threadStopCts.Cancel();
                this.thread.Join();
                this.threadStopCts.Dispose();
            } else {
                this.thread.Abort();
            }
        }

        ~PrismThread()
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
