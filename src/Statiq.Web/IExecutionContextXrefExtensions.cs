using System;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Web
{
    public static class IExecutionContextXrefExtensions
    {
        public static bool TryGetXrefDocument(this IExecutionContext context, string xref, out IDocument document)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            ImmutableArray<IDocument> matches = context.Outputs[nameof(Pipelines.Content)].Flatten()
                .Where(x => x.GetString(WebKeys.Xref)?.Equals(xref, StringComparison.OrdinalIgnoreCase) == true)
                .ToImmutableDocumentArray();
            if (matches.Length > 1)
            {
                throw new ExecutionException($"Multiple ambiguous matching documents found for xref \"{xref}\"");
            }
            if (matches.Length == 1)
            {
                document = matches[0];
                return true;
            }
            document = default;
            return false;
        }

        public static IDocument GetXrefDocument(this IExecutionContext context, string xref) =>
            context.TryGetXrefDocument(xref, out IDocument document) ? document : throw new ExecutionException($"Couldn't find document with xref \"{xref}\"");

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, out string link) =>
            context.TryGetXrefLink(xref, false, out link);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, bool includeHost, out string link)
        {
            if (context.TryGetXrefDocument(xref, out IDocument document))
            {
                link = document.GetLink(includeHost);
                return link != null;
            }
            link = default;
            return false;
        }

        public static string GetXrefLink(this IExecutionContext context, string xref, bool includeHost = false) =>
            context.TryGetXrefLink(xref, includeHost, out string link) ? link : throw new ExecutionException($"Couldn't get link for document with xref \"{xref}\"");
    }
}