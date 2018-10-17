using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Wyam.Hosting.LiveReload;
using Wyam.Hosting.Middleware;

namespace Wyam.Hosting
{
    /// <summary>
    /// An HTTP server that can serve static files from a specified directory on disk.
    /// </summary>
    public class Server : IWebHost
    {
        private readonly IDictionary<string, string> _contentTypes;
        private readonly IWebHost _host;
        private readonly ILoggerProvider _loggerProvider;

        public bool Extensionless { get; }

        public string LocalPath { get; }

        public int Port { get; }

        /// <summary>
        /// The virtual directory at which files are served (or null). This will always
        /// begin with a backslash and end without one.
        /// </summary>
        public string VirtualDirectory { get; }

        public IFeatureCollection ServerFeatures => _host.ServerFeatures;

        public IServiceProvider Services => _host.Services;

        // internal for testing
        internal ConcurrentBag<IReloadClient> LiveReloadClients { get; } = null;

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
            : this(localPath, port, extensionless, virtualDirectory, liveReload, null, loggerProvider)
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
        /// <param name="contentTypes">Additional content types the server should support.</param>
        public Server(string localPath, int port, bool extensionless, string virtualDirectory, bool liveReload, IDictionary<string, string> contentTypes, ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
            _contentTypes = contentTypes;
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
            Port = port <= 0 ? throw new ArgumentException("The port must be greater than 0") : port;
            Extensionless = extensionless;

            if (!string.IsNullOrWhiteSpace(virtualDirectory))
            {
                if (!virtualDirectory.StartsWith("/"))
                {
                    virtualDirectory = "/" + virtualDirectory;
                }
                VirtualDirectory = virtualDirectory.TrimEnd('/');
            }

            if (liveReload)
            {
                LiveReloadClients = new ConcurrentBag<IReloadClient>();
            }

            string currentDirectory = Directory.GetCurrentDirectory();

            _host = new WebHostBuilder()
                .UseContentRoot(currentDirectory)
                .UseWebRoot(Path.Combine(currentDirectory, "wwwroot"))
                .ConfigureLogging(log =>
                {
                    if (loggerProvider != null)
                    {
                        log.AddProvider(loggerProvider);
                    }
                })
                .UseKestrel()
                .UseUrls($"http://localhost:{port}")
                .Configure(ConfigureApp)
                .Build();
        }

        /// <inheritdoc />
        public void Dispose() => _host.Dispose();

        /// <summary>
        /// Start listening.
        /// </summary>
        public void Start() => _host.Start();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken = default(CancellationToken)) => _host.StartAsync(cancellationToken);

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default(CancellationToken)) => _host.StopAsync(cancellationToken);

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

        private void ConfigureApp(IApplicationBuilder app)
        {
            // Configure file providers
            CompositeFileProvider compositeFileProvider = new CompositeFileProvider(
                new PhysicalFileProvider(LocalPath),
                new ManifestEmbeddedFileProvider(typeof(Server).Assembly, "wwwroot"));
            IHostingEnvironment host = app.ApplicationServices.GetService<IHostingEnvironment>();
            host.WebRootFileProvider = compositeFileProvider;

            if (LiveReloadClients != null)
            {
                // Inject LiveReload script tags to HTML documents, needs to run first as it overrides output stream
                app.UseScriptInjection($"{VirtualDirectory ?? string.Empty}/livereload.js?host=localhost&port={Port}");

                // Host ws:// (this also needs to go early in the pipeline so WS can return before virtual directory, etc.)
                //app.MapFleckRoute<ReloadClient>("/livereload", connection =>
                //{
                //    ReloadClient reloadClient = (ReloadClient)connection;
                //    reloadClient.Logger = _loggerProvider?.CreateLogger("LiveReload");
                //    LiveReloadClients.Add(reloadClient);
                //});

                // TODO: Figure out websockets (do I still need Fleck?)
                //app.UseWebSockets();
            }

            // Support for virtual directory
            if (!string.IsNullOrEmpty(VirtualDirectory))
            {
                app.UseVirtualDirectory(VirtualDirectory);
            }

            // Disable caching
            app.UseDisableCache();

            // Support for extensionless URLs
            if (Extensionless)
            {
                // TODO: let the user specify additional default extensions
                app.UseDefaultExtensions(new DefaultExtensionsOptions());
            }

            // Add JSON content type
            // TODO: let the user specify additional content types
            FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".json"] = "application/json";
            if (_contentTypes != null)
            {
                foreach (KeyValuePair<string, string> contentType in _contentTypes)
                {
                    contentTypeProvider.Mappings[contentType.Key.StartsWith(".") ? contentType.Key : "." + contentType.Key] = contentType.Value;
                }
            }

            // Serve up all static files
            // TODO: let the user specify additional default files
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                RequestPath = PathString.Empty,
                FileProvider = compositeFileProvider,
                DefaultFileNames = new List<string> { "index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html" }
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = PathString.Empty,
                FileProvider = compositeFileProvider,
                ServeUnknownFileTypes = true,
                ContentTypeProvider = contentTypeProvider
            });

            //if (LiveReloadClients != null)
            //{
            //    // Host livereload.js (do this last so virtual directory rewriting applies)
            //    Assembly liveReloadAssembly = typeof(ReloadClient).Assembly;
            //    string rootNamespace = typeof(ReloadClient).Namespace;
            //    IFileSystem reloadFilesystem = new EmbeddedResourceFileSystem(liveReloadAssembly, $"{rootNamespace}");
            //    app.UseStaticFiles(new StaticFileOptions
            //    {
            //        RequestPath = PathString.Empty,
            //        FileSystem = reloadFilesystem,
            //        ServeUnknownFileTypes = true,
            //        ContentTypeProvider = contentTypeProvider
            //    });
            //}
        }
    }
}