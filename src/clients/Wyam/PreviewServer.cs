using System;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Hosting;

namespace Wyam
{
    internal static class PreviewServer
    {
        public static Server Start(DirectoryPath path, int port, int liveReloadPort,
            bool forceExtension, DirectoryPath virtualDirectory)
        {
            Server server;
            try
            {
                server = new Server(path.FullPath, port, liveReloadPort, !forceExtension,
                    virtualDirectory?.FullPath, x => Trace.Verbose(x));
                server.Start();
            }
            catch (Exception ex)
            {
                Trace.Critical($"Error while running preview server: {ex.Message}");
                return null;
            }

            Trace.Information($"Preview server listening on port {port} and serving from path {path}"
                + (virtualDirectory == null ? string.Empty : $" with virtual directory {virtualDirectory.FullPath}"));
            if (liveReloadPort > 0)
            {
                Trace.Information($"LiveReload server listening on port {liveReloadPort}");
            }
            return server;
        }
    }
}