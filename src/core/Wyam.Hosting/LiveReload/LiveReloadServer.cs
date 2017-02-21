using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Owin.WebSocket.Extensions;

namespace Wyam.Hosting.LiveReload
{
    internal class LiveReloadServer : IWebHost
    {
        private readonly ConcurrentBag<IReloadClient> _clients = new ConcurrentBag<IReloadClient>();
        private readonly Action<string> _logAction;
        private readonly IWebHost _host;
        
        public LiveReloadServer(int port, Action<string> logAction)
        {
            if (port <= 0)
            {
                throw new ArgumentException("The LiveReload port must be greater than 0");
            }

            _logAction = logAction ?? (_ => { });
            Port = port;

            _host = new WebHostBuilder()
                .ConfigureLogging(log => log.AddProvider(new LogActionLoggerProvider(_logAction)))
                .UseKestrel()
                .UseUrls($"http://localhost:{port}")
                .Configure(builder =>
                {
                    builder.UseWebSockets();
                    builder.UseOwinBuilder(OwinBuilder);
                })
                .Build();
        }

        public int Port { get; }

        // virtual is required to mock for testing
        public virtual IEnumerable<IReloadClient> ReloadClients => _clients;

        public void Start() => _host.Start();

        public void Dispose() => _host.Dispose();

        public IFeatureCollection ServerFeatures => _host.ServerFeatures;

        public IServiceProvider Services => _host.Services;

        public void TriggerReload()
        {
            foreach (IReloadClient client in ReloadClients.Where(x => x.IsConnected))
            {
                client.NotifyOfChanges();
            }
        }

        // This gets applied to both the main server and the LiveReload server
        internal void OwinBuilder(IAppBuilder app)
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
            app.MapFleckRoute<ReloadClient>("/livereload", connection =>
            {
                ReloadClient reloadClient = (ReloadClient) connection;
                reloadClient.LogAction = _logAction;
                _clients.Add(reloadClient);
            });
        }
    }
}