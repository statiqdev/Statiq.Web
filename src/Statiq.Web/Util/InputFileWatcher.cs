using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Web
{
    /// <summary>
    /// A wrapper around <see cref="FileSystemWatcher"/> that invokes a callback action on changes to input files.
    /// </summary>
    public class InputFileWatcher : IDisposable
    {
        public static readonly TimeSpan EventThreshold = TimeSpan.FromMilliseconds(800);

        private static readonly DateTime _zeroFileTime = DateTime.FromFileTime(0);

        private readonly BlockingCollection<string> _changedFiles =
            new BlockingCollection<string>(new ConcurrentQueue<string>());
        private readonly ConcurrentCache<string, DateTime> _fileLastWriteTimes =
            new ConcurrentCache<string, DateTime>(false);
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly string _outputPath;
        private readonly Func<IEnumerable<string>, Task> _callback;

        public InputFileWatcher(
            in NormalizedPath outputDirectory,
            IEnumerable<NormalizedPath> inputDirectories,
            bool includeSubdirectories,
            string filter,
            Func<IEnumerable<string>, Task> callback)
        {
            foreach (string inputDirectory in inputDirectories.Select(x => x.FullPath).Where(Directory.Exists))
            {
                FileSystemWatcher watcher = new FileSystemWatcher
                {
                    Path = inputDirectory,
                    IncludeSubdirectories = includeSubdirectories,
                    Filter = filter,
                    EnableRaisingEvents = true
                };
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;
                _watchers.Add(watcher);
            }
            _outputPath = outputDirectory.FullPath;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            Task.Run(ProcessChangedFilesAsync);
        }

        public void Dispose()
        {
            if (!_changedFiles.IsAddingCompleted)
            {
                // Complete the changed file queue
                try
                {
                    _changedFiles.CompleteAdding();
                }
                catch
                {
                }
                _changedFiles.Dispose();

                // Remove the watcher events
                foreach (FileSystemWatcher watcher in _watchers)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Changed -= OnChanged;
                    watcher.Created -= OnChanged;
                    watcher.Deleted -= OnChanged;
                    watcher.Renamed -= OnRenamed;
                }
            }
        }

        private async Task ProcessChangedFilesAsync()
        {
            string changedFile;
            while ((changedFile = TakeChangedFile()) is object)
            {
                // Wait a little bit of time to let sequences of events fire
                await Task.Delay(EventThreshold / 2);

                // Go ahead and clear out the queue
                List<string> changedFiles = new List<string> { changedFile };
                while (TryTakeChangedFile(out changedFile))
                {
                    changedFiles.Add(changedFile);
                }
                await _callback(changedFiles);
            }
        }

        private string TakeChangedFile()
        {
            if (!_changedFiles.IsCompleted)
            {
                try
                {
                    return _changedFiles.Take();
                }
                catch
                {
                    // The collection was completed or disposed while waiting
                }
            }
            return null;
        }

        private bool TryTakeChangedFile(out string changedFile)
        {
            if (!_changedFiles.IsCompleted)
            {
                try
                {
                    return _changedFiles.TryTake(out changedFile);
                }
                catch
                {
                    // The collection was completed or disposed while waiting
                }
            }
            changedFile = null;
            return false;
        }

        private void OnChanged(object sender, FileSystemEventArgs args)
        {
            DateTime lastWriteTime = GetLastWriteTime(args.FullPath);
            AddAndThrottle(args.FullPath, lastWriteTime);
        }

        private void OnRenamed(object sender, RenamedEventArgs args)
        {
            // Use now as the "write" time for renamed files
            DateTime now = DateTime.Now;
            AddAndThrottle(args.OldFullPath, now);
            AddAndThrottle(args.FullPath, now);
        }

        private static DateTime GetLastWriteTime(string fullPath)
        {
            DateTime lastWriteTime = File.GetLastWriteTime(fullPath);
            return lastWriteTime.Equals(default) || lastWriteTime.Equals(_zeroFileTime) ? DateTime.Now : lastWriteTime;
        }

        // Multiple events are often fired for file changes so we need to throttle them somehow
        // Looking at last write time with a small threshold seems to work well
        private void AddAndThrottle(string fullPath, DateTime lastWriteTime)
        {
            if (!fullPath.StartsWith(_outputPath, StringComparison.OrdinalIgnoreCase))
            {
                _fileLastWriteTimes.AddOrUpdate(
                    fullPath,
                    fp =>
                    {
                        // First time we've seen an event for this file
                        try
                        {
                            _changedFiles.Add(fp);
                        }
                        catch
                        {
                            // Either the collection has completed or is disposed
                        }
                        return lastWriteTime;
                    },
                    (fp, previousWriteTime) =>
                    {
                        // We've seen this file before so only add it if it's been long enough
                        if (lastWriteTime - previousWriteTime > EventThreshold)
                        {
                            try
                            {
                                _changedFiles.Add(fp);
                            }
                            catch
                            {
                                // Either the collection has completed or is disposed
                            }
                        }
                        return lastWriteTime;
                    });
            }
        }
    }
}