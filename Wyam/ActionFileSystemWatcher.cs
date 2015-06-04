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
        private readonly Action<string> _callback;

        public ActionFileSystemWatcher(string path, Action<string> callback)
        {
            _watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
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
            _callback(args.FullPath);
        }
    }
}
