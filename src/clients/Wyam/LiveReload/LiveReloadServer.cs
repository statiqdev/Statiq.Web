using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;

using Owin;
using Owin.WebSocket.Extensions;

using Wyam.Common.Tracing;
using Wyam.Owin;

namespace Wyam.LiveReload
{
    internal class LiveReloadServer : IDisposable
    {
        private readonly ReloadClientServiceLocator _clientServiceLocator;
        private IDisposable _server;

        public LiveReloadServer(ReloadClientServiceLocator clientServiceLocator = null)
        {
            _clientServiceLocator = clientServiceLocator ?? new ReloadClientServiceLocator();
        }

        public void StartStandaloneHost(int port = 35729)
        {
            try
            {
                // This must be 127.0.0.1 due to http.sys hostname verification, LiveReload hardcodes it.
                StartOptions options = new StartOptions("http://127.0.0.1:" + port);
                options.Settings.Add(typeof(ITraceOutputFactory).FullName, typeof(NullTraceOutputFactory).AssemblyQualifiedName);
                _server = WebApp.Start(options, InjectOwinMiddleware);
            }
            catch (Exception ex)
            {
                Trace.Warning($"Error while running the LiveReload server: {ex.Message}");
            }

            Trace.Verbose($"LiveReload server listening on port {port}.");
        }

        public void InjectOwinMiddleware(IAppBuilder app)
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
            app.MapWebSocketRoute<ReloadClient>("/livereload", _clientServiceLocator);
        }

        public void RebuildCompleted(ICollection<string> filesChanged)
        {
            IEnumerable<IReloadClient> clientsToNotify = _clientServiceLocator.ReloadClients;
            foreach (IReloadClient client in clientsToNotify.Where(x => x.IsConnected))
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