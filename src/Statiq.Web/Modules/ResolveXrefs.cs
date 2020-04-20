using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Statiq.Common;
using Statiq.Html;

namespace Statiq.Web.Modules
{
    public class ResolveXrefs : ParallelModule
    {
        private static readonly HtmlParser HtmlParser = new HtmlParser();

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context)
        {
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(context, HtmlParser);
            if (htmlDocument != null)
            {
                // Find and replace "xref:" in links
                bool modifiedDocument = false;
                foreach (IElement element in htmlDocument
                    .GetElementsByTagName("a")
                    .Where(x => x.HasAttribute("href")))
                {
                    string href = element.GetAttribute("href");
                    if (href.StartsWith("xref:") && href.Length > 5)
                    {
                        string xref = href.Substring(5);
                        string queryAndFragment = string.Empty;
                        int queryAndFragmentIndex = xref.IndexOfAny(new[] { '#', '?' });
                        if (queryAndFragmentIndex > 0)
                        {
                            queryAndFragment = xref.Substring(queryAndFragmentIndex);
                            xref = xref.Substring(0, queryAndFragmentIndex);
                        }
                        element.Attributes["href"].Value = context.GetXrefLink(xref) + queryAndFragment;
                        modifiedDocument = true;
                    }
                }

                // Return a new document with the replacements if we performed any
                if (modifiedDocument)
                {
                    using (Stream contentStream = await context.GetContentStreamAsync())
                    {
                        using (StreamWriter writer = contentStream.GetWriter())
                        {
                            htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                            writer.Flush();
                            return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Html)).Yield();
                        }
                    }
                }
            }

            return input.Yield();
        }
    }
}