using System;
using System.Threading.Tasks;

namespace Provausio.Core.Ext
{
    public static class TaskExt
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            // if the original task finishes first, then we're good
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                return await task;
            
            // or else that means the timeout finished first, in which case throw
            throw new TimeoutException();
        }

        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            // if the original task finishes first, then we're good
            if (task != await Task.WhenAny(task, Task.Delay(timeout)))
                throw new TimeoutException();
        }
    }       
}
