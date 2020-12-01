using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web
{
    /// <summary>
    /// A wrapper around <see cref="FileSystemWatcher"/> that invokes a callback action on changes to input files.
    /// </summary>
    public class InputFileWatcher : IDisposable
    {
        private static readonly TimeSpan _eventThreshold = TimeSpan.FromMilliseconds(800);

        private readonly BlockingCollection<string> _changedFiles = new BlockingCollection<string>(new ConcurrentQueue<string>());
        private readonly ConcurrentCache<string, DateTime> _fileLastWriteTimes = new ConcurrentCache<string, DateTime>();
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly string _outputPath;
        private readonly Func<string, Task> _callback;

        public InputFileWatcher(
            in NormalizedPath outputDirectory,
            IEnumerable<NormalizedPath> inputDirectories,
            bool includeSubdirectories,
            string filter,
            Func<string, Task> callback)
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
                watcher.Renamed += OnChanged;
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
                }
            }
        }

        private async Task ProcessChangedFilesAsync()
        {
            string changedFile;
            while ((changedFile = TakeChangedFile()) is object)
            {
                // Wait a little bit of time to let sequences of events fire
                await Task.Delay(_eventThreshold / 2);
                await _callback(changedFile);
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

        private void OnChanged(object sender, FileSystemEventArgs args)
        {
            if (!args.FullPath.StartsWith(_outputPath, StringComparison.OrdinalIgnoreCase))
            {
                // Multiple events are often fired for file changes so we need to throttle them somehow
                // Looking at last write time with a small threshold seems to work well
                _fileLastWriteTimes.AddOrUpdate(
                    args.FullPath,
                    fullPath =>
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(fullPath);
                        try
                        {
                            _changedFiles.Add(args.FullPath);
                        }
                        catch
                        {
                            // Either the collection has completed or is disposed
                        }
                        return lastWriteTime;
                    },
                    (fullPath, previousWriteTime) =>
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(fullPath);
                        if (lastWriteTime - previousWriteTime > _eventThreshold)
                        {
                            try
                            {
                                _changedFiles.Add(args.FullPath);
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
