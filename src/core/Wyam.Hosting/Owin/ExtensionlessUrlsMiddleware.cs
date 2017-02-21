using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;

namespace Wyam.Hosting.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Handles URLs without extensions by checking if a file with an extension exists at the corresponding
    /// path in the file system.
    /// </summary>
    public class ExtensionlessUrlsMiddleware
    {
        private readonly ExtensionlessUrlsOptions _options;
        private readonly AppFunc _next;

        public ExtensionlessUrlsMiddleware(AppFunc next, ExtensionlessUrlsOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (options.FileSystem == null)
            {
                options.FileSystem = new PhysicalFileSystem(".");
            }
            options.DefaultExtensions =
                options.DefaultExtensions.Select(x => x.StartsWith(".") ? x : ("." + x)).ToList();

            _next = next;
            _options = options;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);
            if (IsGetOrHeadMethod(context.Request.Method)
                && !PathEndsInSlash(context.Request.Path))
            {
                // Check if there's a file with the matched extension, and rewrite the request if found
                foreach (string extension in _options.DefaultExtensions)
                {
                    string filePath = Uri.UnescapeDataString(context.Request.Path.ToString()) + extension;
                    IFileInfo fileInfo;
                    if (_options.FileSystem.TryGetFileInfo(filePath, out fileInfo))
                    {
                        context.Request.Path = new PathString(filePath);
                        break;
                    }
                }
            }

            return _next(environment);
        }

        // These methods are from Microsoft.Owin.StaticFiles.Helpers
        private static bool IsGetOrHeadMethod(string method)
        {
            return IsGetMethod(method) || IsHeadMethod(method);
        }

        private static bool IsGetMethod(string method)
        {
            return string.Equals("GET", method, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHeadMethod(string method)
        {
            return string.Equals("HEAD", method, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PathEndsInSlash(PathString path)
        {
            return path.Value.EndsWith("/", StringComparison.Ordinal);
        }
    }
}
