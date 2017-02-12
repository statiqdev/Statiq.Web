using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

using Owin;
using Owin.WebSocket.Extensions;

using Wyam.Common.Tracing;
using Wyam.Server;

namespace Wyam.LiveReload
{
    internal class LiveReloadServer : IDisposable
    {
        private readonly ConcurrentBag<IReloadClient> _clients = new ConcurrentBag<IReloadClient>();
        private HttpServer _server;

        public virtual IEnumerable<IReloadClient> ReloadClients => _clients.ToArray();

        public void StartStandaloneHost(int port = 35729, bool throwExceptions = false)
        {
            try
            {
                _server = new HttpServer();
                _server.StartServer(port, AddHostMiddleware);
            }
            catch (Exception ex)
            {
                Trace.Warning($"Error while running the LiveReload server: {ex.Message}");
                if (throwExceptions)
                {
                    throw;
                }
            }

            Trace.Verbose($"LiveReload server listening on port {port}.");
        }

        public void AddInjectionMiddleware(IAppBuilder app)
        {
            // Inject LR script.
            app.UseLiveReloadScriptInjections("/livereload.js");
        }

        public void AddHostMiddleware(IAppBuilder app)
        {
            // Host livereload.js
            Assembly liveReloadAssembly = typeof(LiveReloadServer).Assembly;
            string rootNamespace = typeof(LiveReloadServer).Namespace;
            IFileSystem reloadFilesystem = new EmbeddedResourceFileSystem(liveReloadAssembly, $"{rootNamespace}");
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = PathString.Empty,
                FileSystem = reloadFilesystem,
                ServeUnknownFileTypes = true
            });

            // Host ws://
            app.MapFleckRoute<ReloadClient>("/livereload", connection => _clients.Add((ReloadClient) connection));
        }

        public void RebuildCompleted(ICollection<string> filesChanged)
        {
            foreach (IReloadClient client in ReloadClients.Where(x => x.IsConnected))
            {
                foreach (string modifiedFile in filesChanged)
                {
                    client.NotifyOfChanges(modifiedFile);
                }
            }
        }

        public void Dispose()
        {
            _server?.Dispose();
        }
    }
}