using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Html;

namespace Statiq.Web.Modules
{
    public class ResolveXrefs : Module
    {
        protected override async Task<IEnumerable<Common.IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Key = source, Value = tag HTML
            ConcurrentDictionary<string, ConcurrentBag<string>> failures =
                new ConcurrentDictionary<string, ConcurrentBag<string>>();

            // Resolve the xrefs in parallel
            IEnumerable<Common.IDocument> outputs = await context.Inputs
                .ParallelSelectAsync(async input => await ResolveDocumentXrefsAsync(input, context, failures));

            // Report failures and throw if there are any
            if (failures.Count > 0)
            {
                int failureCount = failures.Sum(x => x.Value.Count);
                string failureMessage = string.Join(
                    Environment.NewLine,
                    failures.Select(x => $"{x.Key}{Environment.NewLine} - {string.Join(Environment.NewLine + " - ", x.Value)}"));
                context.LogError($"{failureCount} xref resolution failures:{Environment.NewLine}{failureMessage}");
                throw new ExecutionException("Encountered some invalid xrefs");
            }

            return outputs;
        }

        private static async Task<Common.IDocument> ResolveDocumentXrefsAsync(
            Common.IDocument input,
            IExecutionContext context,
            ConcurrentDictionary<string, ConcurrentBag<string>> failures)
        {
            IHtmlDocument htmlDocument = await HtmlHelper.ParseHtmlAsync(input, false);
            if (htmlDocument is object)
            {
                // Find and replace "xref:" in links
                bool modifiedDocument = false;
                bool errors = false;
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
                            // Continue processing so we can report all the failures in a given document
                            failures.AddOrUpdate(
                                input.Source.FullPath,
                                _ => new ConcurrentBag<string> { error },
                                (_, list) =>
                                {
                                    list.Add(error);
                                    return list;
                                });
                            errors = true;
                        }
                        modifiedDocument = true;
                    }
                }

                // Exit if there were errors
                if (errors)
                {
                    return null;
                }

                // Return a new document with the replacements if we performed any
                if (modifiedDocument)
                {
                    using (Stream contentStream = context.GetContentStream())
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