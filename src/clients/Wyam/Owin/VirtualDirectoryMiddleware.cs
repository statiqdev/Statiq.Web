using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;

namespace Wyam.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class VirtualDirectoryMiddleware
    {
        private readonly string _virtualDirectory;
        private readonly AppFunc _next;

        public VirtualDirectoryMiddleware(AppFunc next, string virtualDirectory)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (virtualDirectory == null)
            {
                throw new ArgumentNullException(nameof(virtualDirectory));
            }

            if (!virtualDirectory.StartsWith("/"))
            {
                virtualDirectory = "/" + virtualDirectory;
            }
            virtualDirectory = virtualDirectory.TrimEnd('/');

            _next = next;
            _virtualDirectory = virtualDirectory;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);
            if (context.Request.Path.ToString().StartsWith(_virtualDirectory))
            {
                string realPath = context.Request.Path.ToString().Substring(_virtualDirectory.Length);
                if (realPath == string.Empty)
                {
                    realPath = "/";
                }
                context.Request.Path = new PathString(realPath);
                context.Request.PathBase = new PathString(_virtualDirectory);
                return _next(environment);
            }

            // This isn't under our virtual directory, so it should be a not found
            context.Response.StatusCode = 404;
            context.Response.ReasonPhrase = "Not Under Virtual Directory";
            return context.Response.WriteAsync(string.Empty);
        }
    }
}
