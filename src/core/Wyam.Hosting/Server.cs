using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Owin.WebSocket.Extensions;
using Wyam.Hosting.LiveReload;
using Wyam.Hosting.Owin;

namespace Wyam.Hosting
{
    /// <summary>
    /// An HTTP server that can serve static files from a specified directory on disk.
    /// </summary>
    public class Server : IWebHost
    {
        private readonly ILoggerProvider _loggerProvider;
        private readonly IWebHost _host;

        /// <summary>
        /// Creates the HTTP server.
        /// </summary>
        /// <param name="localPath">The local path to serve files from.</param>
        /// <param name="port">The port the server will serve HTTP requests on.</param>
        public Server(string localPath, int port = 5080)
            : this(localPath, port, true, null, true, null)
        {
        }

        /// <summary>
        /// Creates the HTTP server.
        /// </summary>
        /// <param name="localPath">The local path to serve files from.</param>
        /// <param name="port">The port the server will serve HTTP requests on.</param>
        /// <param name="extensionless"><c>true</c> if the server should support extensionless URLs, <c>false</c> otherwise.</param>
        /// <param name="virtualDirectory">The virtual directory the server should respond to, or <c>null</c> to use the root URL.</param>
        /// <param name="liveReload">Enables support for LiveReload.</param>
        /// <param name="loggerProvider">The logger provider to use.</param>
        public Server(string localPath, int port, bool extensionless, string virtualDirectory, bool liveReload, ILoggerProvider loggerProvider)
        {
            if (localPath == null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }
            if (port <= 0)
            {
                throw new ArgumentException("The port must be greater than 0");
            }

            _loggerProvider = loggerProvider;
            LocalPath = localPath;
            Port = port;
            Extensionless = extensionless;
            VirtualDirectory = virtualDirectory;

            if (liveReload)
            {
                LiveReloadClients = new ConcurrentBag<IReloadClient>();
            }

            _host = new WebHostBuilder()
                .ConfigureLogging(log =>
                {
                    if (loggerProvider != null)
                    {
                        log.AddProvider(loggerProvider);
                    }
                })
                .UseKestrel()
                .UseUrls($"http://localhost:{port}")
                .Configure(builder =>
                {
                    builder.UseWebSockets();
                    builder.UseOwinBuilder(OwinBuilder);
                })
                .Build();
        }

        public string LocalPath { get; }

        public int Port { get; }

        public bool Extensionless { get; }

        public string VirtualDirectory { get; }

        // internal virtual is required to mock for testing
        internal virtual ConcurrentBag<IReloadClient> LiveReloadClients { get; } = null;

        /// <summary>
        /// Start listening.
        /// </summary>
        public void Start() => _host.Start();

        public IFeatureCollection ServerFeatures => _host.ServerFeatures;

        public IServiceProvider Services => _host.Services;

        public void Dispose() => _host.Dispose();

        public void TriggerReload()
        {
            if (LiveReloadClients != null)
            {
                foreach (IReloadClient client in LiveReloadClients.Where(x => x.IsConnected))
                {
                    client.NotifyOfChanges();
                }
            }
        }

        private void OwinBuilder(IAppBuilder app)
        {
            IFileSystem outputFolder = new PhysicalFileSystem(LocalPath);

            // Inject LiveReload script tags to HTML documents, needs to run first as it overrides output stream
            if (LiveReloadClients != null)
            {
                app.UseScriptInjection("/livereload.js");
            }

            // Support for virtual directory
            if (!string.IsNullOrEmpty(VirtualDirectory))
            {
                app.UseVirtualDirectory(VirtualDirectory);
            }

            // Disable caching
            app.Use((c, t) =>
            {
                c.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                c.Response.Headers.Append("Pragma", "no-cache");
                c.Response.Headers.Append("Expires", "0");
                return t();
            });

            // Support for extensionless URLs
            if (Extensionless)
            {
                app.UseExtensionlessUrls(new ExtensionlessUrlsOptions
                {
                    FileSystem = outputFolder
                });
            }

            // Serve up all static files
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                RequestPath = PathString.Empty,
                FileSystem = outputFolder,
                DefaultFileNames = new List<string> { "index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html" }
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = PathString.Empty,
                FileSystem = outputFolder,
                ServeUnknownFileTypes = true
            });

            // Add the LiveReload middleware
            if (LiveReloadClients != null)
            {
                // Host livereload.js
                Assembly liveReloadAssembly = typeof(ReloadClient).Assembly;
                string rootNamespace = typeof(ReloadClient).Namespace;
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
                    ReloadClient reloadClient = (ReloadClient)connection;
                    reloadClient.Logger = _loggerProvider?.CreateLogger("LiveReload");
                    LiveReloadClients.Add(reloadClient);
                });
            }
        }
    }
}
