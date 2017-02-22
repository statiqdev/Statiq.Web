using System;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Hosting;
using Wyam.Tracing;

namespace Wyam
{
    internal static class PreviewServer
    {
        public static Server Start(DirectoryPath path, int port, bool forceExtension, DirectoryPath virtualDirectory, bool liveReload)
        {
            Server server;
            try
            {
                server = new Server(path.FullPath, port, !forceExtension, virtualDirectory?.FullPath, liveReload, new TraceLoggerProvider());
                server.Start();
            }
            catch (Exception ex)
            {
                Trace.Critical($"Error while running preview server: {ex.Message}");
                return null;
            }

            Trace.Information($"Preview server listening on port {port} and serving from path {path}"
                + (virtualDirectory == null ? string.Empty : $" with virtual directory {virtualDirectory.FullPath}")
                + (liveReload ? " and LiveReload support" : string.Empty));
            return server;
        }
    }
}