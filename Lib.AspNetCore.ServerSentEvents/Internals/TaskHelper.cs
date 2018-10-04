using System.Threading;
using System.Threading.Tasks;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class TaskHelper
    {
        #region Methods
        internal static Task WaitAsync(this CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> cancelationTaskCompletionSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(taskCompletionSource => ((TaskCompletionSource<bool>)taskCompletionSource).SetResult(true), cancelationTaskCompletionSource);

            return cancellationToken.IsCancellationRequested ? Task.CompletedTask : cancelationTaskCompletionSource.Task;
        }
        #endregion
    }
}
