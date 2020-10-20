using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public class ValidateAbsoluteLinks : ValidateLinks
    {
        private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        private readonly ConcurrentCache<Uri, Task<string>> _resultCache = new ConcurrentCache<Uri, Task<string>>();

        /// <summary>
        /// Validating absolute links is expensive, so this should be disabled by default.
        /// </summary>
        public override LogLevel LogLevel { get; set; } = LogLevel.None;

        public override Task BeforeEngineExecutionAsync(IEngine engine, Guid executionId)
        {
            _resultCache.Clear();
            return Task.CompletedTask;
        }

        protected override async Task AnalyzeAsync(IHtmlDocument htmlDocument, Common.IDocument document, IAnalyzerContext context)
        {
            // Validate links in parallel
            await GetLinks(htmlDocument, document, context, true)
                .ParallelForEachAsync(async x => await ValidateLinkAsync(x, document, context));
        }

        // Internal for testing
        private async Task ValidateLinkAsync((Uri, IEnumerable<IElement>) link, Common.IDocument document, IAnalyzerContext context)
        {
            string result = await _resultCache.GetOrAdd(link.Item1, async uri =>
            {
                if (uri.ToString().StartsWith("//"))
                {
                    // Double-slash link means use http:// or https:// depending on current protocol
                    // Try as http first, then https
                    if (!Uri.TryCreate($"http:{uri.OriginalString}", UriKind.Absolute, out Uri absoluteUri))
                    {
                        return "Invalid protocol-relative URI";
                    }
                    if (await ValidateLinkAsync(absoluteUri, context) != HttpStatusCode.OK)
                    {
                        UriBuilder uriBuilder = new UriBuilder(absoluteUri);
                        bool hadDefaultPort = uriBuilder.Uri.IsDefaultPort;
                        uriBuilder.Scheme = Uri.UriSchemeHttps;
                        uriBuilder.Port = hadDefaultPort ? -1 : uriBuilder.Port;
                        HttpStatusCode statusCode = await ValidateLinkAsync(uriBuilder.Uri, context);
                        if (statusCode != HttpStatusCode.OK)
                        {
                            return $"Could not validate protocol-relative URI - {(int)statusCode} {statusCode}";
                        }
                    }
                    return null;
                }

                // Only validate http and https schemes
                if (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    HttpStatusCode statusCode = await ValidateLinkAsync(uri, context);
                    if (statusCode != HttpStatusCode.OK)
                    {
                        return $"Could not validate absolute URI - {(int)statusCode} {statusCode}";
                    }
                }
                else
                {
                    context.Logger.LogDebug($"Skipping validation for absolute link {uri}: unsupported scheme {uri.Scheme}.");
                }
                return null;
            });
            if (result is object)
            {
                AddAnalyzerResult(result, link.Item2, document, context);
            }
        }

        // Perform request as HEAD then try one more time as GET
        private static async Task<HttpStatusCode> ValidateLinkAsync(Uri uri, IAnalyzerContext context)
        {
            if (await ValidateLinkAsync(uri, HttpMethod.Head, context) == HttpStatusCode.OK)
            {
                return HttpStatusCode.OK;
            }
            return await ValidateLinkAsync(uri, HttpMethod.Get, context);
        }

        private static async Task<HttpStatusCode> ValidateLinkAsync(Uri uri, HttpMethod method, IAnalyzerContext context)
        {
            try
            {
                HttpResponseMessage response = await context.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(method, uri), 2);

                // Even with exponential back-off we have TooManyRequests, just skip, since we have to assume it's valid.
                if (response.StatusCode == TooManyRequests)
                {
                    context.Logger.LogDebug($"Skipping validation for absolute link {uri}: too many requests have been issued so can't reliably test.");
                    return HttpStatusCode.OK;
                }

                // We don't use IsSuccessStatusCode, we consider in this case 300's valid.
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    context.Logger.LogDebug($"Validation failure for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                    return response.StatusCode;
                }

                // We don't bother disposing of the response in this case. Due to advice from here: https://stackoverflow.com/questions/15705092/do-httpclient-and-httpclienthandler-have-to-be-disposed
                context.Logger.LogDebug($"Validation success for absolute link {method} {uri}: returned status code {(int)response.StatusCode} {response.StatusCode}");
                return HttpStatusCode.OK;
            }
            catch (TaskCanceledException ex)
            {
                context.Logger.LogDebug($"Skipping validation for absolute link {method} {uri} due to timeout: {ex}.");
                return HttpStatusCode.OK;
            }
            catch (ArgumentException ex)
            {
                context.Logger.LogDebug($"Skipping validation for absolute link {method} {uri} due to invalid uri: {ex}.");
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                context.Logger.LogDebug($"Skipping validation for absolute link {method} {uri} due to unknown error: {ex}.");
                return HttpStatusCode.OK;
            }
        }
    }
}
