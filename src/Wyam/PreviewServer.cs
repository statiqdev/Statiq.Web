using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Owin;

namespace Wyam
{
    internal static class PreviewServer
    {
        public static IDisposable Start(DirectoryPath path, int port, bool forceExtension)
        {
            IDisposable server;
            try
            {
                StartOptions options = new StartOptions("http://localhost:" + port);

                // Disable built-in owin tracing by using a null trace output
                // http://stackoverflow.com/questions/17948363/tracelistener-in-owin-self-hosting
                options.Settings.Add(typeof(ITraceOutputFactory).FullName, typeof(NullTraceOutputFactory).AssemblyQualifiedName);

                server = WebApp.Start(options, app =>
                {
                    Microsoft.Owin.FileSystems.IFileSystem outputFolder = new PhysicalFileSystem(path.FullPath);

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
                        DefaultFileNames = new List<string> { "index.html", "index.htm", "home.html", "home.htm", "default.html", "default.html" }
                    });
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = PathString.Empty,
                        FileSystem = outputFolder,
                        ServeUnknownFileTypes = true
                    });
                });
            }
            catch (Exception ex)
            {
                Trace.Critical("Error while running preview server: {0}", ex.Message);
                return null;
            }

            Trace.Information("Preview server listening on port {0} and serving from path {1}", port, path);
            return server;
        }

        private class NullTraceOutputFactory : ITraceOutputFactory
        {
            public TextWriter Create(string outputFile)
            {
                return StreamWriter.Null;
            }
        }
    }
}