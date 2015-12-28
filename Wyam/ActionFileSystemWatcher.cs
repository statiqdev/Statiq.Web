using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam
{
    public class ActionFileSystemWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _outputPath;
        private readonly Action<string> _callback;

        public ActionFileSystemWatcher(string outputPath, string path, bool includeSubdirectories, string filter, Action<string> callback)
        {
            _watcher = new FileSystemWatcher
            {
                Path = path,
                IncludeSubdirectories = includeSubdirectories,
                Filter = filter,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _outputPath = outputPath;
            _callback = callback;
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnChanged;
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
