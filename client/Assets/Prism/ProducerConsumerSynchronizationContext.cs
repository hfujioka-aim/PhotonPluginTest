using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Prism
{
    public sealed class ConcurrentQueueSynchronizationContext:
        ProducerConsumerSynchronizationContext<ConcurrentQueue<(SendOrPostCallback, object, ManualResetEventSlim)>>
    {
        public override SynchronizationContext CreateCopy()
            => new ConcurrentQueueSynchronizationContext();
    }

    public sealed class ConcurrentBagSynchronizationContext:
        ProducerConsumerSynchronizationContext<ConcurrentBag<(SendOrPostCallback, object, ManualResetEventSlim)>>
    {
        public override SynchronizationContext CreateCopy()
            => new ConcurrentBagSynchronizationContext();
    }

    public class ProducerConsumerSynchronizationContext<T>: SynchronizationContext, IDisposable
        where T: IProducerConsumerCollection<(SendOrPostCallback d, object state, ManualResetEventSlim mre)>, new()
    {
        public int ManagedThreadId { get; protected set; }

        public ProducerConsumerSynchronizationContext()
            : this(Thread.CurrentThread.ManagedThreadId) { }

        public ProducerConsumerSynchronizationContext(int workerThreadId)
        {
            this.ManagedThreadId = workerThreadId;
        }

        public override SynchronizationContext CreateCopy()
            => new ProducerConsumerSynchronizationContext<T>();

        public override void Post(SendOrPostCallback d, object state)
            => this.Post(d, state, null);

        public override void Send(SendOrPostCallback d, object state)
        {
            if (Thread.CurrentThread.ManagedThreadId == this.ManagedThreadId) {
                d(state);
                return;
            }

            using (var mre = new ManualResetEventSlim(false)) {
                this.Post(d, state, mre);
                mre.Wait();
            }
        }

        protected T TaskQueue { get; } = new T();

        private int queueCount = 0;
        private SemaphoreSlim sem { get; } = new SemaphoreSlim(0);

        protected void Post(SendOrPostCallback d, object state, ManualResetEventSlim mre)
        {
            if (!this.TaskQueue.TryAdd((d, state, mre))) {
                throw new InvalidOperationException();
            }
            if (Interlocked.Increment(ref this.queueCount) == 1) {
                this.sem.Release();
            }
        }

        public bool DispatchAsyncTasks()
        {
            var isExec = false;
            while (this.TaskQueue.TryTake(out var task)) {
                try {
                    isExec = true;
                    if (Interlocked.Decrement(ref this.queueCount) == 0) {
                        if (!this.sem.Wait(0)) {
                            throw new InvalidOperationException();
                        }
                    }
                    task.d(task.state);
                } finally {
                    task.mre?.Set();
                }
            }

            return isExec;
        }

        private int opCount = 0;

        public override void OperationStarted()
            => Interlocked.Increment(ref this.opCount);

        public override void OperationCompleted()
            => Interlocked.Decrement(ref this.opCount);

        public virtual bool HasPendingTasks()
            => this.queueCount > 0 || this.opCount > 0 || this.TaskQueue.Count != 0 || this.Wait(0);

        public void Wait()
        {
            this.sem.Wait();
            this.sem.Release();
        }

        public bool Wait(int ms)
        {
            var isWait = this.sem.Wait(ms);
            if (isWait) {
                this.sem.Release();
            }
            return isWait;
        }

        public void Wait(CancellationToken token)
        {
            this.sem.Wait(token);
            this.sem.Release();
        }

        public bool Wait(int ms, CancellationToken token)
        {
            var isWait = this.sem.Wait(ms, token);
            if (isWait) {
                this.sem.Release();
            }
            return isWait;
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
                this.sem.Dispose();
                (this.TaskQueue as IDisposable)?.Dispose();
            }
        }

        ~ProducerConsumerSynchronizationContext()
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
