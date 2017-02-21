using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Hosting.LiveReload;
using Wyam.Hosting.Owin;

namespace Wyam.Hosting
{
    /// <summary>
    /// An HTTP server that can serve static files from a specified directory on disk.
    /// </summary>
    public class Server : IWebHost
    {
        private readonly Action<string> _logAction;
        private readonly IWebHost _host;
        private readonly LiveReloadServer _liveReloadServer;

        /// <summary>
        /// Creates the HTTP server.
        /// </summary>
        /// <param name="localPath">The local path to serve files from.</param>
        /// <param name="port">The port the server will serve HTTP requests on.</param>
        /// <param name="liveReloadPort">
        /// The port the server will use for LiveReload requests. Set to 0 to disable
        /// LiveReload capabilities.
        /// </param>
        /// <param name="extensionless"><c>true</c> if the server should support extensionless URLs, <c>false</c> otherwise.</param>
        /// <param name="virtualDirectory">The virtual directory the server should respond to, or <c>null</c> to use the root URL.</param>
        /// <param name="logAction">An action to call for logging messages.</param>
        public Server(string localPath, int port = 5080, 
            int liveReloadPort = 35729, bool extensionless = true, string virtualDirectory = null,
            Action<string> logAction = null)
        {
            if (localPath == null)
            {
                throw new ArgumentNullException(nameof(localPath));
            }
            if (port <= 0)
            {
                throw new ArgumentException("The port must be greater than 0");
            }

            _logAction = logAction ?? (_ => {});
            LocalPath = localPath;
            Port = port;
            Extensionless = extensionless;
            VirtualDirectory = virtualDirectory;

            if (liveReloadPort > 0)
            {
                _liveReloadServer = new LiveReloadServer(liveReloadPort, _logAction);
            }

            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://localhost:{port}")
                .Configure(builder => builder.UseOwinBuilder(OwinBuilder))
                .Build();
        }

        public string LocalPath { get; }

        public int Port { get; }
        
        public bool Extensionless { get; }

        public string VirtualDirectory { get; }

        public int LiveReloadPort => _liveReloadServer?.Port ?? 0;

        /// <summary>
        /// Start listening.
        /// </summary>
        public void Start()
        {
            _liveReloadServer?.Start();
            _host.Start();
        }

        public IFeatureCollection ServerFeatures => _host.ServerFeatures;

        public IServiceProvider Services => _host.Services;

        public void Dispose()
        {
            _liveReloadServer?.Dispose();
            _host.Dispose();
        }

        public void TriggerReload() => _liveReloadServer?.TriggerReload();

        private void OwinBuilder(IAppBuilder app)
        {
            IFileSystem outputFolder = new PhysicalFileSystem(LocalPath);
            
            // LiveReload support
            if (_liveReloadServer != null)
            {
                // Inject LiveReload script tags to HTML documents, needs to run first as it overrides output stream
                app.UseScriptInjection("/livereload.js");

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
        }
    }
}
