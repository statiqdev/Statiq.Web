using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Html;

namespace Statiq.Web.Modules
{
    public class ResolveXrefs : Module
    {
        private static readonly HtmlParser HtmlParser = new HtmlParser();

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Resolve the xrefs in parallel
            IEnumerable<Common.IDocument> outputs = await context.Inputs.ParallelSelectAsync(async input => await ResolveDocumentXrefsAsync(input, context));

            // Throw if there were any errors (as evidenced by a null document return)
            if (outputs.Any(x => x == null))
            {
                throw new ExecutionException("Encountered some invalid xrefs");
            }

            return outputs;
        }

        private static async Task<Common.IDocument> ResolveDocumentXrefsAsync(Common.IDocument input, IExecutionContext context)
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
                        if (context.TryGetXrefLink(xref, out string xrefLink, out string error))
                        {
                            element.Attributes["href"].Value = xrefLink + queryAndFragment;
                        }
                        else
                        {
                            input.LogError(error);
                            return null;
                        }
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
                            return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Html));
                        }
                    }
                }
            }

            return input;
        }
    }
}