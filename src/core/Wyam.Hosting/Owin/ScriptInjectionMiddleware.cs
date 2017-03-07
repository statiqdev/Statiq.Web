using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Owin;

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
                scriptUrls.Select(x => $@"<script type=""text/javascript"" src=""{x}""></script>"));
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);

            // Intercept the original stream
            Stream originalBody = context.Response.Body;
            MemoryStream interceptedBody = new MemoryStream();
            context.Response.Body = interceptedBody;

            // Run the middleware
            await _next(environment);

            // Write the buffer to the output stream
            interceptedBody.Position = 0;
            if (string.Equals(context.Response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
            {
                StreamReader reader = new StreamReader(interceptedBody);
                string body = await reader.ReadToEndAsync();
                int closingTag = body.LastIndexOf("</body>", StringComparison.Ordinal);
                if (closingTag > -1)
                {
                    interceptedBody = new MemoryStream(reader.CurrentEncoding.GetBytes(body.Insert(closingTag, _injectionCode)));
                    context.Response.ContentLength = interceptedBody.Length;
                }
            }
            interceptedBody.Position = 0;
            interceptedBody.CopyTo(originalBody);
            context.Response.Body = originalBody;
        }
    }
}