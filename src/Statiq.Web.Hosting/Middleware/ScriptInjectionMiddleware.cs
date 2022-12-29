using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
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
            scriptUrls.ThrowIfNull(nameof(scriptUrls));
            _next = next.ThrowIfNull(nameof(next));
            _injectionCode = string.Join(
                Environment.NewLine,
                scriptUrls.Select(x => $@"<script type=""text/javascript"" src=""{x}""></script>"));
        }

        /// <inheritdoc />
        public async Task InvokeAsync(HttpContext context)
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
                string document = await reader.ReadToEndAsync();
                int injectionPosition = GetInjectionPosition(document);
                interceptedBody = new MemoryStream(reader.CurrentEncoding.GetBytes(document.Insert(injectionPosition, _injectionCode)));
                context.Response.ContentLength = interceptedBody.Length;
            }
            interceptedBody.Position = 0;
            await interceptedBody.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }

        private int GetInjectionPosition(string document)
        {
            // Try for the </body> tag first
            int position = document.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

            // If we didn't find a body, try for the </html> tag
            if (position == -1)
            {
                position = document.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);
            }

            // If we didn't find a body or html, just inject at the end
            if (position == -1)
            {
                position = document.Length;
            }

            return position;
        }
    }
}