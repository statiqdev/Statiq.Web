using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

using Owin;
using Owin.WebSocket.Extensions;

namespace Wyam.LiveReload
{
    internal class LiveReloadServer : IDisposable
    {
        private readonly ReloadClientServiceLocator _clientServiceLocator;

        public LiveReloadServer()
        {
            _clientServiceLocator = new ReloadClientServiceLocator();
        }

        public void InjectOwinMiddleware(IAppBuilder app)
        {
            // Host livereload.js
            var liveReloadAssembly = typeof(LiveReloadServer).Assembly;
            var rootNamespace = typeof(LiveReloadServer).Namespace;
            var reloadFilesystem = new EmbeddedResourceFileSystem(liveReloadAssembly, $"{rootNamespace}");
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = PathString.Empty,
                FileSystem = reloadFilesystem,
                ServeUnknownFileTypes = true
            });

            // Host ws://
            app.MapWebSocketRoute<ReloadClient>("/livereload", _clientServiceLocator);
        }

        public void RebuildCompleted(ICollection<string> filesChanged)
        {
            var clientsToNotify = _clientServiceLocator?.ReloadClients ?? Enumerable.Empty<ReloadClient>();
            foreach (var client in clientsToNotify.Where(x => x.IsConnected))
            {
                foreach (var modifiedFile in filesChanged)
                {
                    client.NotifyOfChanges(modifiedFile);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}