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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet;
using Wyam.Hosting.LiveReload;
using Wyam.Hosting.Middleware;

namespace Wyam.Hosting
{
    /// <summary>
    /// An HTTP server that can serve static files from a specified directory on disk.
    /// </summary>
    public class Server : IWebHost
    {
        private static readonly Func<string[], DefaultExtensionsOptions> GetExtensionOptions = (extensions) =>
        {
            if (extensions != null && extensions.Length > 0)
            {
                return new DefaultExtensionsOptions
                {
                    Extensions = extensions
                };
            }

            return new DefaultExtensionsOptions();
        };

        private static readonly Func<string, PreviewServerOptions> GetPreviewServerOptions = (localPath) =>
            new PreviewServerOptions
            {
                LocalPath = localPath
            };

        private readonly IDictionary<string, string> _contentTypes;
        private readonly IWebHost _host;
        private readonly ILoggerProvider _loggerProvider;
        public string[] DefaultExtensions { get; }

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

        // internal virtual is required to mock for testing
        internal virtual ConcurrentBag<IReloadClient> LiveReloadClients { get; } = null;

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
            if (port <= 0)
            {
                throw new ArgumentException("The port must be greater than 0");
            }

            _loggerProvider = loggerProvider;
            _contentTypes = contentTypes;
            LocalPath = localPath ?? throw new ArgumentNullException(nameof(localPath));
            Port = port;
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
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    DefaultExtensionsOptions defaultExtensionsOptions = GetExtensionOptions(DefaultExtensions);
                    PreviewServerOptions previewServerOptions = GetPreviewServerOptions(LocalPath);
                    services
                    .AddSingleton(defaultExtensionsOptions)
                    .AddSingleton(previewServerOptions);
                })
                .Build();  // .Build() once the AspNetCore packages are updated to 2.x
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
    }
}