using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Statiq.Common;

namespace Statiq.Web
{
    /// <summary>
    /// A wrapper around <see cref="FileSystemWatcher"/> that invokes a callback action on changes to input files.
    /// </summary>
    public class InputFileWatcher : IDisposable
    {
        private static readonly TimeSpan _eventThreshold = TimeSpan.FromMilliseconds(500);

        private readonly ConcurrentCache<string, DateTime> _fileLastWriteTimes = new ConcurrentCache<string, DateTime>();
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly string _outputPath;
        private readonly Action<string> _callback;

        public InputFileWatcher(in NormalizedPath outputDirectory, IEnumerable<NormalizedPath> inputDirectories, bool includeSubdirectories, string filter, Action<string> callback)
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
                _watchers.Add(watcher);
            }
            _outputPath = outputDirectory.FullPath;
            _callback = callback;
        }

        public void Dispose()
        {
            foreach (FileSystemWatcher watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnChanged;
                watcher.Created -= OnChanged;
            }
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
                        _callback(args.FullPath);
                        return lastWriteTime;
                    },
                    (fullPath, previousWriteTime) =>
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(fullPath);
                        if (lastWriteTime - previousWriteTime > _eventThreshold)
                        {
                            _callback(args.FullPath);
                        }
                        return lastWriteTime;
                    });
            }
        }
    }
}
