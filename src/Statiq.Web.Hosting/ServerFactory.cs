using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Statiq.Web.Hosting
{
    public class ServerFactory
    {
        public bool Extensionless { get; set; } = true;

        public string VirtualDirectory { get; set; }

        public bool LiveReload { get; set; } = true;

        public Dictionary<string, string> ContentTypes { get; }
            = new Dictionary<string, string>();

        public Dictionary<string, string> CustomHeaders { get; }
            = new Dictionary<string, string>();

        public IList<ILoggerProvider> LoggerProviders { get; }
            = new List<ILoggerProvider>();

        public ServerFactory WithExtensionless(bool extensionless = true)
        {
            Extensionless = extensionless;
            return this;
        }

        public ServerFactory WithVirtualDirectory(string virtualDirectory)
        {
            VirtualDirectory = virtualDirectory;
            return this;
        }

        public ServerFactory WithLiveReload(bool liveReload = true)
        {
            LiveReload = liveReload;
            return this;
        }

        public ServerFactory WithContentType(string extension, string contentType)
        {
            ContentTypes[extension] = contentType;
            return this;
        }

        public ServerFactory WithContentTypes(IReadOnlyDictionary<string, string> contentTypes)
        {
            if (contentTypes is object)
            {
                foreach (KeyValuePair<string, string> contentType in contentTypes)
                {
                    ContentTypes[contentType.Key] = contentType.Value;
                }
            }
            return this;
        }

        public ServerFactory WithCustomHeader(string name, string value)
        {
            CustomHeaders[name] = value;
            return this;
        }

        public ServerFactory WithCustomHeaders(IReadOnlyDictionary<string, string> customHeaders)
        {
            if (customHeaders is object)
            {
                foreach (KeyValuePair<string, string> customHeader in customHeaders)
                {
                    CustomHeaders[customHeader.Key] = customHeader.Value;
                }
            }
            return this;
        }

        public ServerFactory WithLoggerProvider(ILoggerProvider loggerProvider)
        {
            if (loggerProvider is object)
            {
                LoggerProviders.Add(loggerProvider);
            }
            return this;
        }

        public ServerFactory WithLoggerProviders(IEnumerable<ILoggerProvider> loggerProviders)
        {
            if (loggerProviders is object)
            {
                foreach (ILoggerProvider loggerProvider in loggerProviders)
                {
                    WithLoggerProvider(loggerProvider);
                }
            }
            return this;
        }

        public Server CreateServer(string localPath, int port = 5080) => new Server(
            localPath,
            port,
            Extensionless,
            VirtualDirectory,
            LiveReload,
            ContentTypes,
            CustomHeaders,
            LoggerProviders);
    }
}