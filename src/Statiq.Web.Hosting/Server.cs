using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Web.Hosting.LiveReload;
using Statiq.Web.Hosting.Middleware;

namespace Statiq.Web.Hosting
{
    /// <summary>
    /// An HTTP server that can serve static files from a specified directory on disk.
    /// </summary>
    public class Server : IWebHost
    {
        private readonly IDictionary<string, string> _contentTypes;
        private readonly IWebHost _host;
        private readonly LiveReloadServer _liveReloadServer;

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
        /// <param name="loggerProviders">The logger providers to use.</param>
        public Server(string localPath, int port, bool extensionless, string virtualDirectory, bool liveReload, IEnumerable<ILoggerProvider> loggerProviders)
            : this(localPath, port, extensionless, virtualDirectory, liveReload, null, loggerProviders)
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
        /// <param name="loggerProviders">The logger providers to use.</param>
        /// <param name="contentTypes">Additional content types the server should support.</param>
        public Server(string localPath, int port, bool extensionless, string virtualDirectory, bool liveReload, IDictionary<string, string> contentTypes, IEnumerable<ILoggerProvider> loggerProviders)
        {
            _contentTypes = contentTypes;
            LocalPath = localPath.ThrowIfNull(nameof(localPath));
            Port = port <= 0 ? throw new ArgumentException("The port must be greater than 0") : port;
            Extensionless = extensionless;

            if (!string.IsNullOrWhiteSpace(virtualDirectory))
            {
                VirtualDirectory = VirtualDirectoryMiddleware.NormalizeVirtualDirectory(virtualDirectory);
            }

            if (liveReload)
            {
                _liveReloadServer = new LiveReloadServer();
            }

            string currentDirectory = Directory.GetCurrentDirectory();

            _host = new WebHostBuilder()
                .UseContentRoot(currentDirectory)
                .UseWebRoot(Path.Combine(currentDirectory, "wwwroot"))
                .ConfigureLogging(log =>
                {
                    if (loggerProviders is object)
                    {
                        foreach (ILoggerProvider loggerProvider in loggerProviders)
                        {
                            if (loggerProvider is object)
                            {
                                log.AddProvider(loggerProvider);
                            }
                        }
                    }
                })
                .UseKestrel()
                .ConfigureKestrel(x => x.ListenAnyIP(port))
                .ConfigureServices(ConfigureServices)
                .Configure(ConfigureApp)
                .Build();
        }

        /// <inheritdoc />
        public void Dispose() => _host.Dispose();

        /// <inheritdoc />
        public void Start() => _host.Start();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken = default) => _host.StartAsync(cancellationToken);

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default) => _host.StopAsync(cancellationToken);

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                // Only loopback proxies are allowed by default, clear that restriction
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            // Configure file providers
            CompositeFileProvider compositeFileProvider = new CompositeFileProvider(
                new PhysicalFileProvider(LocalPath),
                new ManifestEmbeddedFileProvider(typeof(Server).Assembly, "wwwroot"));
            IWebHostEnvironment host = app.ApplicationServices.GetService<IWebHostEnvironment>();
            host.WebRootFileProvider = compositeFileProvider;

            if (_liveReloadServer is object)
            {
                // Inject LiveReload script tags to HTML documents, needs to run first as it overrides output stream
                app.UseScriptInjection($"{VirtualDirectory ?? string.Empty}/livereload.js?host=localhost&port={Port}");

                // Turn on web sockets and the live reload middleware
                app.UseWebSockets();
                app.UseLiveReload(_liveReloadServer);
            }

            // Support for virtual directory
            if (!string.IsNullOrEmpty(VirtualDirectory))
            {
                app.UseVirtualDirectory(VirtualDirectory);
            }

            // Disable caching
            app.UseDisableCache();

            // Process forwarded headers from proxies, etc. (I.e. GitHub Codespaces port forwarding)
            // See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-3.1
            app.UseForwardedHeaders();

            // Support for extensionless URLs
            if (Extensionless)
            {
                // TODO: let the user specify additional default extensions
                app.UseDefaultExtensions(new DefaultExtensionsOptions());
            }

            // Use our large set of mappings and add any additional ones
            FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider(MediaTypes.ExtensionMappings);
            if (_contentTypes is object)
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
        }

        public async Task TriggerReloadAsync()
        {
            if (_liveReloadServer is object)
            {
                await _liveReloadServer.SendReloadMessageAsync();
            }
        }
    }
}