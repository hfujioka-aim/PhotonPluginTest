using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

namespace Prism
{
    internal static class TaskExtensions
    {
#pragma warning disable IDE1006 // 命名スタイル
        public static async ValueTask Unwrap(this Task<ValueTask> task)
            => await (await task.ConfigureAwait(false)).ConfigureAwait(false);

        public static async ValueTask<T> Unwrap<T>(this Task<ValueTask<T>> task)
            => await (await task.ConfigureAwait(false)).ConfigureAwait(false);

        public static async ValueTask Unwrap(this ValueTask<Task> task)
            => await (await task.ConfigureAwait(false)).ConfigureAwait(false);

        public static async ValueTask<T> Unwrap<T>(this ValueTask<Task<T>> task)
            => await (await task.ConfigureAwait(false)).ConfigureAwait(false);

        public static async UniTask Unwrap(this Task<UniTask> task, bool continueOnCapturedContext = true)
            => await await task.ConfigureAwait(continueOnCapturedContext);

        public static async UniTask<T> Unwrap<T>(this Task<UniTask<T>> task, bool continueOnCapturedContext = true)
            => await await task.ConfigureAwait(continueOnCapturedContext);

        public static async UniTask Unwrap(this UniTask<Task> task, bool continueOnCapturedContext = true)
            => await (await task).ConfigureAwait(continueOnCapturedContext);

        public static async UniTask<T> Unwrap<T>(this UniTask<Task<T>> task, bool continueOnCapturedContext = true)
            => await (await task).ConfigureAwait(continueOnCapturedContext);

        public static async UniTask Unwrap(this ValueTask<UniTask> task, bool continueOnCapturedContext = true)
            => await await task.ConfigureAwait(continueOnCapturedContext);

        public static async UniTask<T> Unwrap<T>(this ValueTask<UniTask<T>> task, bool continueOnCapturedContext = true)
            => await await task.ConfigureAwait(continueOnCapturedContext);

        public static async UniTask Unwrap(this UniTask<ValueTask> task, bool continueOnCapturedContext = true)
            => await (await task).ConfigureAwait(continueOnCapturedContext);

        public static async UniTask<T> Unwrap<T>(this UniTask<ValueTask<T>> task, bool continueOnCapturedContext = true)
            => await (await task).ConfigureAwait(continueOnCapturedContext);
#pragma warning restore IDE1006 // 命名スタイル
    }
}
