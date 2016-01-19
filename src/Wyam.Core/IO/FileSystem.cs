using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    // TODO: Figure out the best way to specify that this is an input file/directory and find the appropriate root - this is where the merge should happen
    internal sealed class FileSystem : IFileSystem
    {
        public IFile GetFile(FilePath path)
        {
            return new File(path);
        }
        
        public IDirectory GetDirectory(DirectoryPath path)
        {
            return new Directory(path);
        }

        // *** Retry logic

        private static readonly TimeSpan InitialInterval = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan IntervalDelta = TimeSpan.FromMilliseconds(200);
        private const int RetryCount = 3;

        public static T Retry<T>(Func<T> func)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    TimeSpan? interval = ShouldRetry(retryCount, ex);
                    if (!interval.HasValue)
                    {
                        throw;
                    }
                    Thread.Sleep(interval.Value);
                }
                retryCount++;
            }
        }

        public static void Retry(Action action)
        {
            Retry<object>(() =>
            {
                action();
                return null;
            });
        }

        private static TimeSpan? ShouldRetry(int retryCount, Exception exception) =>
            (exception is IOException || exception is UnauthorizedAccessException) && retryCount < RetryCount
                ? (TimeSpan?)InitialInterval.Add(TimeSpan.FromMilliseconds(IntervalDelta.TotalMilliseconds * retryCount)) : null;
    }
}
