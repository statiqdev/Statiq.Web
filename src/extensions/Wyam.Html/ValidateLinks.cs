using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using IDocument = Wyam.Common.Documents.IDocument;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Html
{
    /// <summary>
    /// Performs link validation for HTML elements such as anchors, images, and other resources.
    /// </summary>
    /// <remarks>
    /// Both relative and absolute links can be validated, though only relative links are checked
    /// by default due to the time it takes to check absolute links.
    /// </remarks>
    /// <category>Input/Output</category>
    public class ValidateLinks : IModule
    {
        private bool _validateAbsoluteLinks;
        private bool _validateRelativeLinks = true;
        private bool _asError;

        /// <summary>
        /// Validates absolute (often external) links. This may add a significant delay to your
        /// generation process so it's recommended absolute links are only checked periodically.
        /// The default behavior is not to check absolute links. Also note that false positive
        /// failures are common when validating external links so any links that fail the check
        /// should be subsequently checked manually.
        /// </summary>
        /// <param name="validateAbsoluteLinks"><c>true</c> to validate absolute links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateAbsoluteLinks(bool validateAbsoluteLinks = true)
        {
            _validateAbsoluteLinks = validateAbsoluteLinks;
            return this;
        }

        /// <summary>
        /// Validates relative links, which is activated by default.
        /// </summary>
        /// <param name="validateRelativeLinks"><c>true</c> to validate relative links.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks ValidateRelativeLinks(bool validateRelativeLinks = true)
        {
            _validateRelativeLinks = validateRelativeLinks;
            return this;
        }

        /// <summary>
        /// When the validation process is complete, all the validation failures will
        /// be output as warnings. This method can be used to report all of the failures
        /// as errors instead (possibly breaking the generation).
        /// </summary>
        /// <param name="asError"><c>true</c> to report failures as an error.</param>
        /// <returns>The current module instance.</returns>
        public ValidateLinks AsError(bool asError = true)
        {
            _asError = asError;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
#pragma warning disable RCS1163 // Unused parameter.
            // Handle invalid HTTPS certificates and allow alternate security protocols (see http://stackoverflow.com/a/5670954/807064)
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
#pragma warning restore RCS1163 // Unused parameter.

            // Key = link, Value = source, tag HTML
            ConcurrentDictionary<string, ConcurrentBag<(string documentSource, string outerHtml)>> links =
                new ConcurrentDictionary<string, ConcurrentBag<(string documentSource, string outerHtml)>>();

            // Key = source, Value = tag HTML
            ConcurrentDictionary<string, ConcurrentBag<string>> failures = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            // Gather all links
            HtmlParser parser = new HtmlParser();
            context.ParallelForEach(inputs, input => GatherLinks(input, parser, links));

            // Perform validation
            Parallel.ForEach(links, link =>
            {
                // Attempt to parse the URI
                if (!Uri.TryCreate(link.Key, UriKind.RelativeOrAbsolute, out Uri uri))
                {
                    AddOrUpdateFailure(link.Value, failures);
                }

                // Adjustment for double-slash link prefix which means use http:// or https:// depending on current protocol
                // The Uri class treats these as relative, but they're really absolute
                if (uri.ToString().StartsWith("//") && !Uri.TryCreate($"http:{link.Key}", UriKind.Absolute, out uri))
                {
                    AddOrUpdateFailure(link.Value, failures);
                }

                // Relative
                if (!uri.IsAbsoluteUri && _validateRelativeLinks && !ValidateRelativeLink(uri, context))
                {
                    AddOrUpdateFailure(link.Value, failures);
                }

                // Absolute
                if (uri.IsAbsoluteUri && _validateAbsoluteLinks && !ValidateAbsoluteLink(uri))
                {
                    AddOrUpdateFailure(link.Value, failures);
                }
            });

            // Report failures
            if (failures.Count > 0)
            {
                int failureCount = failures.Sum(x => x.Value.Count);
                string failureMessage = string.Join(
                    Environment.NewLine,
                    failures.Select(x => $"{x.Key}{Environment.NewLine} - {string.Join(Environment.NewLine + " - ", x.Value)}"));
                Trace.TraceEvent(
                    _asError ? TraceEventType.Error : TraceEventType.Warning,
                    $"{failureCount} link validation failures:{Environment.NewLine}{failureMessage}");
            }

            return inputs;
        }

        // Internal for testing
        internal static void GatherLinks(IDocument input, HtmlParser parser, ConcurrentDictionary<string, ConcurrentBag<(string source, string outerHtml)>> links)
        {
            IHtmlDocument htmlDocument = input.ParseHtml(parser);
            if (htmlDocument != null)
            {
                // Links
                foreach (IElement element in htmlDocument.Links)
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, input.SourceString(), links);
                }

                // Link element
                foreach (IElement element in htmlDocument.GetElementsByTagName("link").Where(x => x.HasAttribute("href")))
                {
                    AddOrUpdateLink(element.GetAttribute("href"), element, input.SourceString(), links);
                }

                // Images
                foreach (IHtmlImageElement element in htmlDocument.Images)
                {
                    AddOrUpdateLink(element.GetAttribute("src"), element, input.SourceString(), links);
                }

                // Scripts
                foreach (IHtmlScriptElement element in htmlDocument.Scripts)
                {
                    AddOrUpdateLink(element.Source, element, input.SourceString(), links);
                }
            }
        }

        // Internal for testing
        internal static bool ValidateRelativeLink(Uri uri, IExecutionContext context)
        {
            List<FilePath> checkPaths = new List<FilePath>();

            // Remove the query string and fragment, if any
            string normalizedPath = uri.ToString();
            if (normalizedPath.Contains("#"))
            {
                normalizedPath = normalizedPath.Remove(normalizedPath.IndexOf("#", StringComparison.Ordinal));
            }
            if (normalizedPath.Contains("?"))
            {
                normalizedPath = normalizedPath.Remove(normalizedPath.IndexOf("?", StringComparison.Ordinal));
            }
            normalizedPath = Uri.UnescapeDataString(normalizedPath);
            if (normalizedPath?.Length == 0)
            {
                return true;
            }

            // Remove the link root if there is one and remove the preceding slash
            if (context.Settings.DirectoryPath(Keys.LinkRoot) != null
                && normalizedPath.StartsWith(context.Settings.DirectoryPath(Keys.LinkRoot).FullPath))
            {
                normalizedPath = normalizedPath.Substring(context.Settings.DirectoryPath(Keys.LinkRoot).FullPath.Length);
            }
            if (normalizedPath.StartsWith("/"))
            {
                normalizedPath = normalizedPath.Length > 1 ? normalizedPath.Substring(1) : string.Empty;
            }

            // Add the base path
            if (normalizedPath != string.Empty)
            {
                checkPaths.Add(new FilePath(normalizedPath));
            }

            // Add filenames
            checkPaths.AddRange(LinkGenerator.DefaultHidePages.Select(x => new FilePath(normalizedPath?.Length == 0 ? x : $"{normalizedPath}/{x}")));

            // Add extensions
            checkPaths.AddRange(LinkGenerator.DefaultHideExtensions.SelectMany(x => checkPaths.Select(y => y.AppendExtension(x))).ToArray());

            // Check all the candidate paths
            FilePath validatedPath = checkPaths.Find(x =>
            {
                IFile outputFile;
                try
                {
                    outputFile = context.FileSystem.GetOutputFile(x);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"Could not validate path {x.FullPath} for relative link {uri}: {ex.Message}");
                    return false;
                }

                return outputFile.Exists;
            });

            if (validatedPath != null)
            {
                Trace.Verbose($"Validated relative link {uri} at {validatedPath.FullPath}");
                return true;
            }

            // Check the absolute URL just in case the user is using some sort of CNAME or something.
            if (Uri.TryCreate(context.GetLink() + uri, UriKind.Absolute, out Uri absoluteCheckUri))
            {
                return ValidateAbsoluteLink(absoluteCheckUri);
            }

            Trace.Verbose($"Validation failure for relative link {uri}: could not find output file at any of {string.Join(", ", checkPaths.Select(x => x.FullPath))}");
            return false;
        }

        // Internal for testing
        internal static bool ValidateAbsoluteLink(Uri uri)
        {
            // If this is a mailto: link, it's sufficient just that it passed the Uri validation.
            if (uri.Scheme == Uri.UriSchemeMailto)
            {
                return true;
            }

            // Perform request as HEAD
            bool result = ValidateAbsoluteLink(uri, "HEAD");

            if (result)
            {
                return true;
            }

            // Try one more time as GET
            return ValidateAbsoluteLink(uri, "GET");
        }

        private static bool ValidateAbsoluteLink(Uri uri, string method)
        {
            // Create a request
            HttpWebRequest request = null;
            try
            {
                request = WebRequest.CreateHttp(uri);
                request.Method = method;

                // Set request properties
                request.Timeout = 60000; // 60 seconds
            }
            catch (NotSupportedException ex)
            {
                Trace.Warning($"Skipping absolute link {uri}: {ex.Message}");
                return true;
            }

            if (request == null)
            {
                Trace.Warning($"Skipping absolute link {uri}: only HTTP/HTTPS links are validated");
                return true;
            }

            HttpWebResponse response = null;
            try
            {
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException)
                {
                    response = null;
                }
                catch (OperationCanceledException)
                {
                    response = null;
                }

                // Check the status code
                if (response != null)
                {
                    if ((int)response.StatusCode >= 100 && (int)response.StatusCode < 400)
                    {
                        Trace.Verbose($"Validated absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                        return true;
                    }

                    Trace.Verbose($"Validation failure for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                    return false;
                }

                Trace.Verbose($"Validation failure for absolute link {method} {uri}: failed to receive a response.");
                return false;
            }
            finally
            {
                response?.Close();
            }
        }

        private static void AddOrUpdateLink(string link, IElement element, string source, ConcurrentDictionary<string, ConcurrentBag<(string source, string outerHtml)>> links)
        {
            if (string.IsNullOrEmpty(link))
            {
                return;
            }

            links.AddOrUpdate(
                link,
                _ => new ConcurrentBag<(string, string)> { (source, ((IElement)element.Clone(false)).OuterHtml) },
                (_, list) =>
                {
                    list.Add((source, ((IElement)element.Clone(false)).OuterHtml));
                    return list;
                });
        }

        private static void AddOrUpdateFailure(ConcurrentBag<(string source, string outerHtml)> links, ConcurrentDictionary<string, ConcurrentBag<string>> failures)
        {
            foreach ((string source, string outerHtml) in links)
            {
                if (source == null)
                {
                    Trace.Warning($"Validation failure for link: unknown file for {outerHtml}.");
                    continue;
                }

                failures.AddOrUpdate(
                    source,
                    _ => new ConcurrentBag<string> { outerHtml },
                    (_, list) =>
                    {
                        list.Add(outerHtml);
                        return list;
                    });
            }
        }
    }
}