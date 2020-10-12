using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Html;

namespace Statiq.Web
{
    public class ValidateRelativeLinks : ValidateLinks
    {
        private readonly HashSet<string> _relativeOutputPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public override LogLevel LogLevel => LogLevel.Warning;

        public override async Task AnalyzeAsync(IAnalyzerContext context)
        {
            // Get existing relative output paths
            _relativeOutputPaths.Clear();
            foreach (NormalizedPath relativeOutputPath in context.FileSystem
                .GetOutputDirectory()
                .GetFiles(System.IO.SearchOption.AllDirectories)
                .Select(x => context.FileSystem.GetRelativeOutputPath(x.Path)))
            {
                _relativeOutputPaths.Add(relativeOutputPath.FullPath.TrimStart('/'));
                _relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, false)).TrimStart('/'));
                _relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, false, true)).TrimStart('/'));
                _relativeOutputPaths.Add(context.GetLink(context.GetLink(relativeOutputPath, null, NormalizedPath.Null, false, true, true)).TrimStart('/'));
            }

            await base.AnalyzeAsync(context);
        }

        protected override Task AnalyzeAsync(IHtmlDocument htmlDocument, Common.IDocument document, IAnalyzerContext context)
        {
            // Validate links in parallel
            GetLinks(htmlDocument, document, context, false)
                .AsParallel()
                .ForAll(x => ValidateLink(x, document, context));
            return Task.CompletedTask;
        }

        private void ValidateLink((Uri, IEnumerable<IElement>) link, Common.IDocument document, IAnalyzerContext context)
        {
            // Remove the query string and fragment, if any
            string normalizedPath = link.Item1.ToString();
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
                // Intra-page link, nothing more to validate
                return;
            }

            // Remove the link root if there is one and remove the preceding slash
            if (!context.Settings.GetPath(Keys.LinkRoot).IsNull
                && normalizedPath.StartsWith(context.Settings.GetPath(Keys.LinkRoot).FullPath))
            {
                normalizedPath = normalizedPath.Substring(context.Settings.GetPath(Keys.LinkRoot).FullPath.Length);
            }
            normalizedPath = normalizedPath.TrimStart('/');

            // See if it's in the output paths
            if (!_relativeOutputPaths.Contains(normalizedPath))
            {
                AddAnalyzerResult("Could not validate relative link", link.Item2, document, context);
            }
        }
    }
}
