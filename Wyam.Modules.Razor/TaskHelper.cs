using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Common/TaskHelper.cs
    internal static class TaskHelper
    {
        /// <summary>
        /// Waits for the task to complete and throws the first faulting exception if the task is faulted.
        /// It preserves the original stack trace when throwing the exception.
        /// </summary>
        /// <remarks>
        /// Invoking this method is equivalent to calling Wait() on the <paramref name="task" /> if it is not completed.
        /// </remarks>
        public static void WaitAndThrowIfFaulted(Task task)
        {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to complete and throws the first faulting exception if the task is faulted.
        /// It preserves the original stack trace when throwing the exception.
        /// </summary>
        /// <remarks>
        /// Invoking this method is equivalent to calling <see cref="Task{TResult}.Result"/> on the
        /// <paramref name="task"/> if it is not completed.
        /// </remarks>
        public static TVal WaitAndThrowIfFaulted<TVal>(Task<TVal> task)
        {
            return task.GetAwaiter().GetResult();
        }
    }
}
