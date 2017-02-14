using System;
using System.Collections.Generic;

using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

using Owin;

using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Owin;
using Wyam.Server;

namespace Wyam
{
    internal static class PreviewServer
    {
        public static IDisposable Start(DirectoryPath path, int port, bool forceExtension, DirectoryPath virtualDirectory)
        {
            HttpServer server;
            try
            {
                server = new HttpServer();
                server.StartServer(port, app => ConfigureOwin(app, path, forceExtension, virtualDirectory));
            }
            catch (Exception ex)
            {
                Trace.Critical($"Error while running preview server: {ex.Message}");
                return null;
            }

            Trace.Information($"Preview server listening on port {port} and serving from path {path}"
                              + (virtualDirectory == null ? string.Empty : $" with virtual directory {virtualDirectory.FullPath}"));
            return server;
        }

        internal static void ConfigureOwin(IAppBuilder app, DirectoryPath path, bool forceExtension, DirectoryPath virtualDirectory)
        {
            Microsoft.Owin.FileSystems.IFileSystem outputFolder = new PhysicalFileSystem(path.FullPath);

            // Support for virtual directory
            if (virtualDirectory != null)
            {
                app.UseVirtualDirectory(virtualDirectory.FullPath);
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
            if (!forceExtension)
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
                DefaultFileNames = new List<string> {"index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html"}
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