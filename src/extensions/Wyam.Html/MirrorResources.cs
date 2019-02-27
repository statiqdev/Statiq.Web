using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Polly;
using Polly.Retry;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Html
{
    /// <summary>
    /// Mirrors external <c>link</c> and <c>script</c> resources locally.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For each input document, looks for <c>link</c> and <c>script</c> elements that link to external
    /// resources and copies them locally, replacing the <c>link</c> and <c>script</c> links to the local path.
    /// To prevent local mirroring for specific <c>link</c> and <c>script</c> elements, add a
    /// <c>data-no-mirror</c> attribute.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category>Input/Output</category>
    public class MirrorResources : IModule
    {
        private const int MaxAbsoluteLinkRetry = 5;
        private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        private readonly Func<Uri, FilePath> _pathFunc;

        /// <summary>
        /// Mirrors external resources locally. By default, resources will be copied into the output folder
        /// under the <c>mirror</c> directory.
        /// </summary>
        public MirrorResources()
            : this(x => $"mirror/{x.Host}{x.LocalPath}")
        {
        }

        /// <summary>
        /// Mirrors external resources locally, specifying the output location where they should be copied.
        /// </summary>
        /// <param name="pathFunc">A function that specifies where downloaded external resources should be copied to.</param>
        public MirrorResources(Func<Uri, FilePath> pathFunc)
        {
            _pathFunc = pathFunc ?? throw new ArgumentNullException(nameof(pathFunc));
        }

        public IEnumerable<Common.Documents.IDocument> Execute(IReadOnlyList<Common.Documents.IDocument> inputs, IExecutionContext context)
        {
#pragma warning disable RCS1163 // Unused parameter.
            // Handle invalid HTTPS certificates and allow alternate security protocols (see http://stackoverflow.com/a/5670954/807064)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
#pragma warning restore RCS1163 // Unused parameter.

            // Cache downloaded resources
            Dictionary<string, string> mirrorCache = new Dictionary<string, string>();

            // Iterate the input documents synchronously so we don't download the same resource more than once
            HtmlParser parser = new HtmlParser();
            return inputs.Select(context, input =>
            {
                IHtmlDocument htmlDocument = input.ParseHtml(parser);
                if (htmlDocument != null)
                {
                    bool modifiedDocument = false;

                    // Link element
                    foreach (IElement element in htmlDocument
                        .GetElementsByTagName("link")
                        .Where(x => x.HasAttribute("href") && !x.HasAttribute("data-no-mirror")))
                    {
                        string replacement = DownloadAndReplace(element.GetAttribute("href"), element, mirrorCache, context);
                        if (replacement != null)
                        {
                            element.Attributes["href"].Value = replacement;
                            modifiedDocument = true;
                        }
                    }

                    // Scripts
                    foreach (IHtmlScriptElement element in htmlDocument.Scripts
                        .Where(x => !string.IsNullOrEmpty(x.Source) && !x.HasAttribute("data-no-mirror")))
                    {
                        string replacement = DownloadAndReplace(element.Source, element, mirrorCache, context);
                        if (replacement != null)
                        {
                            element.Source = replacement;
                            modifiedDocument = true;
                        }
                    }

                    // Return a new document with the replacements if we performed any
                    if (modifiedDocument)
                    {
                        Stream contentStream = context.GetContentStream();
                        using (StreamWriter writer = contentStream.GetWriter())
                        {
                            htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                            writer.Flush();
                            return context.GetDocument(input, contentStream);
                        }
                    }
                }

                return input;
            });
        }

        private string DownloadAndReplace(string source, IElement element, Dictionary<string, string> mirrorCache, IExecutionContext context)
        {
            if (mirrorCache.TryGetValue(source, out string cachedValue))
            {
                return cachedValue;
            }

            // Get the destination path and link
            if (source.StartsWith("//"))
            {
                source = $"http:{source}";
            }
            if (!Uri.TryCreate(source, UriKind.Absolute, out Uri uri))
            {
                // Not absolute
                return null;
            }
            FilePath path = _pathFunc(uri);
            if (path == null)
            {
                throw new Exception($"Null resource mirror path for {source}");
            }
            string link = context.GetLink(path);

            // Download the resource, but only if we haven't already written it to disk
            IFile outputFile = context.FileSystem.GetOutputFile(path);
            if (!outputFile.Exists)
            {
                Common.Tracing.Trace.Verbose($"Downloading resource from {uri} to {path.FullPath}");

                // Retry with exponential backoff links. This helps with websites like GitHub that will give us a 429 -- TooManyRequests.
                RetryPolicy<HttpResponseMessage> retryPolicy = Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(r => r.StatusCode == TooManyRequests)
                    .WaitAndRetryAsync(MaxAbsoluteLinkRetry, attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)));
                HttpResponseMessage response = retryPolicy.ExecuteAsync(async () =>
                {
                    using (HttpClient httpClient = context.CreateHttpClient())
                    {
                        return await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri)).ConfigureAwait(false);
                    }
                }).Result;
                response.EnsureSuccessStatusCode();

                // Copy the result to output
                using (Stream outputStream = outputFile.OpenWrite())
                {
                    response.Content.CopyToAsync(outputStream).Wait();
                }
            }

            mirrorCache.Add(source, link);
            return link;
        }
    }
}
