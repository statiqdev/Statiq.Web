using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Wyam.Hosting.LiveReload;

namespace Wyam.Hosting.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Injects one or more script references into an HTML document just before the closing body element.
    /// </summary>
    public class ScriptInjectionMiddleware
    {
        private readonly AppFunc _next;
        private readonly string _injectionCode;

        public ScriptInjectionMiddleware(AppFunc next, params string[] scriptUrls)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (scriptUrls == null)
            {
                throw new ArgumentNullException(nameof(scriptUrls));
            }

            _next = next;
            _injectionCode = string.Join(Environment.NewLine,
                scriptUrls.Select(x => $@"<script type=""text/javascript"" src=""{x}"" />"));
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);
            if(string.Equals(context.Response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Body = new BodyCloseInjectionStream(_injectionCode, context.Response.Body, Encoding.UTF8);
            }
            return _next(environment);
        }
    }
}
