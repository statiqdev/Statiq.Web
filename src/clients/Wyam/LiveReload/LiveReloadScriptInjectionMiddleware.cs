using System;
using System.IO;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

using Microsoft.Owin;

namespace Wyam.LiveReload
{
    internal class LiveReloadScriptInjectionMiddleware : OwinMiddleware
    {
        private readonly string _scriptPath;

        internal HtmlParser HtmlParser { get; set; } = new HtmlParser();

        public LiveReloadScriptInjectionMiddleware(OwinMiddleware next, string scriptPath) : base(next)
        {
            _scriptPath = scriptPath;
        }

        public override async Task Invoke(IOwinContext context)
        {
            Stream originalBody = context.Response.Body;
            MemoryStream interceptedBody = new MemoryStream();
            context.Response.Body = interceptedBody;

            await Next.Invoke(context);

            if (IsHtmlDocument(context))
            {
                interceptedBody.Position = 0;
                IHtmlDocument document = HtmlParser.Parse(interceptedBody);

                IElement script = document.CreateElement("script");
                script.SetAttribute("type", "text/javascript");
                script.SetAttribute("src", _scriptPath);
                document.Body.Append(script);

                MemoryStream newContentBuffer = new MemoryStream();
                StreamWriter writer = new StreamWriter(newContentBuffer);

                document.ToHtml(writer, new AutoSelectedMarkupFormatter());
                writer.Flush();

                context.Response.ContentLength = newContentBuffer.Length;
                newContentBuffer.Position = 0;
                newContentBuffer.CopyTo(originalBody);

                context.Response.Body = originalBody;
            }
            else
            {
                interceptedBody.Position = 0;
                interceptedBody.CopyTo(originalBody);

                context.Response.Body = originalBody;
            }
        }

        private bool IsHtmlDocument(IOwinContext context)
        {
            const string rfc2854Type = "text/html";
            string contentType = context.Response.ContentType;
            return string.Equals(contentType, rfc2854Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}