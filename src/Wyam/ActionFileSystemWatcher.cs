using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam
{
    public class ActionFileSystemWatcher : IDisposable
    {
        private readonly List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private readonly string _outputPath;
        private readonly Action<string> _callback;

        public ActionFileSystemWatcher(DirectoryPath outputDirectory, IEnumerable<DirectoryPath> inputDirectories, bool includeSubdirectories, string filter, Action<string> callback)
        {
            foreach (DirectoryPath inputDirectory in inputDirectories)
            {
                FileSystemWatcher watcher = new FileSystemWatcher
                {
                    Path = inputDirectory.FullPath,
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
                _callback(args.FullPath);
            }
        }
    }
}
