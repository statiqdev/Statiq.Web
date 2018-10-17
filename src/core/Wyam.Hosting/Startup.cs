using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Wyam.Hosting.Middleware;

using Wyam.Hosting.Middleware;

namespace Wyam.Hosting
{
    internal class Startup
    {
        private readonly DefaultExtensionsOptions _defaultExtensionsOptions;
        private readonly IHostingEnvironment _environment;
        private readonly PreviewServerOptions _previewServerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="previewServerOptions">The preview server options.</param>
        /// <param name="defaultExtensionsOptions">The default extensions options.</param>
        public Startup(
            IHostingEnvironment environment,
            PreviewServerOptions previewServerOptions,
            DefaultExtensionsOptions defaultExtensionsOptions)
        {
            _environment = environment;
            _previewServerOptions = previewServerOptions;
            _defaultExtensionsOptions = defaultExtensionsOptions;
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();

            //// Add JSON content type
            FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();
            //contentTypeProvider.Mappings[".json"] = "application/json";
            //if (_contentTypes != null)
            //{
            //    foreach (KeyValuePair<string, string> contentType in _contentTypes)
            //    {
            //        contentTypeProvider.Mappings[contentType.Key.StartsWith(".") ? contentType.Key : "." + contentType.Key] = contentType.Value;
            //    }
            //}

            // Configure the file providers
            app.Map(_previewServerOptions.LocalPath, ConfigureFileServer);

            // Inject the live reload script
            app.UseScriptInjection("/livereload.js");

            // IFileSystem reloadFilesystem = new EmbeddedResourceFileSystem(ContentAssembly, ContentNamespace);
            app.UseDisableCache()
                .UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = PathString.Empty,
                    // FileSystem = outputFolder,
                    ServeUnknownFileTypes = true,
                    ContentTypeProvider = contentTypeProvider
                })
                .UseFileServer(new FileServerOptions
                {
                    EnableDefaultFiles = true,
                    EnableDirectoryBrowsing = true,
                    StaticFileOptions =
                    {
                        ServeUnknownFileTypes = true
                    }
                });

            // if (LiveReloadClients != null)
            // {
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
            // }
        }

        private void ConfigureFileServer(IApplicationBuilder app)
        {
            IList<string> defaultExtensions = _defaultExtensionsOptions.Extensions;
            if (defaultExtensions != null && defaultExtensions.Count > 0)
            {
                app.UseDefaultExtensions(new DefaultExtensionsOptions
                {
                    Extensions = defaultExtensions
                });
            }

            app.UseFileServer(new FileServerOptions
            {
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    ServeUnknownFileTypes = true,
                },
            });
        }
    }
}