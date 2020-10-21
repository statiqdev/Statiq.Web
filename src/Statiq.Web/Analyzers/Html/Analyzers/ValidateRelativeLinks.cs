using System;
using System.Collections.Generic;
using System.Linq;
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
                _relativeOutputPaths.Add(Uri.UnescapeDataString(relativeOutputPath.FullPath).TrimStart('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, false))).TrimStart('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, false, true))).TrimStart('/'));
                _relativeOutputPaths.Add(Uri.UnescapeDataString(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, true))).TrimStart('/'));
            }

            await base.AnalyzeAsync(context);
        }

        protected override Task AnalyzeAsync(IHtmlDocument htmlDocument, Common.IDocument document, IAnalyzerContext context)
        {
            // Validate links in parallel
            foreach ((string, IEnumerable<IElement>) link in GetLinks(htmlDocument, document, context, false))
            {
                ValidateLink(link, document, context);
            }
            /*
            GetLinks(htmlDocument, document, context, false)
                .AsParallel()
                .ForAll(x => ValidateLink(x, document, context));
            */
            return Task.CompletedTask;
        }

        private void ValidateLink((string, IEnumerable<IElement>) linkAndElements, Common.IDocument document, IAnalyzerContext context)
        {
            // Unescape the path since we're comparing against unescaped links
            string link = Uri.UnescapeDataString(linkAndElements.Item1);

            // Remove the link root if there is one and remove the preceding slash
            if (!context.Settings.GetPath(Keys.LinkRoot).IsNull
                && link.StartsWith(context.Settings.GetPath(Keys.LinkRoot).FullPath))
            {
                link = link.Substring(context.Settings.GetPath(Keys.LinkRoot).FullPath.Length);
            }
            link = link.TrimStart('/');

            // If an intra-page link or link to root, nothing more to validate
            if (link.IsNullOrEmpty())
            {
                return;
            }

            // See if it's in the output paths
            if (!_relativeOutputPaths.Contains(link))
            {
                AddAnalyzerResult("Could not validate relative link", linkAndElements.Item2, document, context);
            }
        }
    }
}
