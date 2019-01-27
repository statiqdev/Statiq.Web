using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Wyam.Hosting.Middleware
{
    /// <summary>
    /// Injects one or more script references into an HTML document just before the closing body element.
    /// </summary>
    internal class ScriptInjectionMiddleware
    {
        private readonly string _injectionCode;
        private readonly RequestDelegate _next;

        public ScriptInjectionMiddleware(RequestDelegate next, params string[] scriptUrls)
        {
            if (scriptUrls == null)
            {
                throw new ArgumentNullException(nameof(scriptUrls));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _injectionCode = string.Join(
                Environment.NewLine,
                scriptUrls.Select(x => $@"<script type=""text/javascript"" src=""{x}""></script>"));
        }

        /// <inheritdoc />
        public async Task Invoke(HttpContext context)
        {
            // Intercept the original stream
            Stream originalBody = context.Response.Body;
            MemoryStream interceptedBody = new MemoryStream();
            context.Response.Body = interceptedBody;

            // Run the middleware
            await _next(context);

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