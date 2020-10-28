using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public class ValidateRelativeLinks : ValidateLinks
    {
        private readonly HashSet<string> _relativeOutputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public override LogLevel LogLevel { get; set; } = LogLevel.Warning;

        private int _count;

        public override async Task AnalyzeAsync(IAnalyzerContext context)
        {
            // Get existing relative output paths
            // Unescape here since we also unescape during the test below to the escapeness matches
            _relativeOutputPaths.Clear();
            foreach (NormalizedPath relativeOutputPath in context.FileSystem
                .GetOutputDirectory()
                .GetFiles(System.IO.SearchOption.AllDirectories)
                .Select(x => context.FileSystem.GetRelativeOutputPath(x.Path)))
            {
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(relativeOutputPath, null, context.Settings.GetPath(Keys.LinkRoot), false, false, false)).Trim('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(relativeOutputPath, null, context.Settings.GetPath(Keys.LinkRoot), false, true, false)).Trim('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(relativeOutputPath, null, context.Settings.GetPath(Keys.LinkRoot), false, false, true)).Trim('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(relativeOutputPath, null, context.Settings.GetPath(Keys.LinkRoot), false, true, true)).Trim('/'));
            }

            _count = 0;
            await base.AnalyzeAsync(context);
            if (_count > 0)
            {
                context.AddAnalyzerResult(null, $"{_count} total relative links could not be validated");
            }
        }

        protected override Task AnalyzeAsync(IHtmlDocument htmlDocument, Common.IDocument document, IAnalyzerContext context)
        {
            int count = 0;
            foreach ((string, IEnumerable<IElement>) link in GetLinks(htmlDocument, document, context, false))
            {
                if (ValidateLink(link, document, context))
                {
                    count++;
                }
            }
            Interlocked.Add(ref _count, count);
            return Task.CompletedTask;
        }

        private bool ValidateLink((string, IEnumerable<IElement>) linkAndElements, Common.IDocument document, IAnalyzerContext context)
        {
            // Unescape the path since we're comparing against unescaped links
            string link = Uri.UnescapeDataString(linkAndElements.Item1);

            // Remove a preceding slash
            link = link.Trim('/');

            // If an intra-page link or link to root, nothing more to validate
            if (link.IsNullOrEmpty())
            {
                return false;
            }

            // See if it's in the output paths
            if (!_relativeOutputPaths.Contains(link))
            {
                AddAnalyzerResult("Could not validate relative link", linkAndElements.Item2, document, context);
                return true;
            }

            return false;
        }
    }
}
