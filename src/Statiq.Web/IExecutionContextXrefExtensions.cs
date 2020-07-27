using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web
{
    public static class IExecutionContextXrefExtensions
    {
        public static bool TryGetXrefDocument(this IExecutionContext context, string xref, out IDocument document, out string error)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            DocumentList<IDocument> documents = context.Outputs.FromPipeline(nameof(Pipelines.Content)).Flatten();
            List<(NormalizedPath, string, IDocument)> xrefs = documents.Select(x => (x.Source, x.GetString(WebKeys.Xref), x)).ToList();
            ImmutableArray<IDocument> matches = documents
                .Where(x => x.GetString(WebKeys.Xref)?.Equals(xref, StringComparison.OrdinalIgnoreCase) == true)
                .ToImmutableDocumentArray();

            if (matches.Length == 1)
            {
                document = matches[0];
                error = default;
                return true;
            }

            document = default;
            error = matches.Length > 1
                ? $"Multiple ambiguous matching documents found for xref \"{xref}\""
                : $"Couldn't find document with xref \"{xref}\"";
            return false;
        }

        public static bool TryGetXrefDocument(this IExecutionContext context, string xref, out IDocument document) =>
            context.TryGetXrefDocument(xref, out document, out string _);

        public static IDocument GetXrefDocument(this IExecutionContext context, string xref) =>
            context.TryGetXrefDocument(xref, out IDocument document, out string error) ? document : throw new ExecutionException(error);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, bool includeHost, out string link, out string error)
        {
            if (context.TryGetXrefDocument(xref, out IDocument document, out error))
            {
                link = document.GetLink(includeHost);
                return link != null;
            }
            link = default;
            return false;
        }

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, out string link, out string error) =>
            context.TryGetXrefLink(xref, false, out link, out error);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, bool includeHost, out string link) =>
            context.TryGetXrefLink(xref, includeHost, out link, out string _);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, out string link) =>
            context.TryGetXrefLink(xref, false, out link, out string _);

        public static string GetXrefLink(this IExecutionContext context, string xref, bool includeHost = false) =>
            context.TryGetXrefLink(xref, includeHost, out string link, out string error) ? link : throw new ExecutionException(error);
    }
}