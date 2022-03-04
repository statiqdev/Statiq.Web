using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Web
{
    public static class IExecutionContextXrefExtensions
    {
        public static bool TryGetXrefDocument(
            this IExecutionContext context,
            string xref,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            out IDocument document,
            out string error)
        {
            context.ThrowIfNull(nameof(context));
            xrefMappings.ThrowIfNull(nameof(xrefMappings));

            // Does the xref exist?
            if (!xrefMappings.TryGetValue(xref, out ICollection<IDocument> matches) || matches.Count == 0)
            {
                document = default;
                error = $"Couldn't find document with xref \"{xref}\"";
                return false;
            }

            // Is there only one?
            if (matches.Count > 1)
            {
                document = default;
                error = $"Multiple ambiguous matching documents found for xref \"{xref}\"";
                return false;
            }

            // Success
            document = matches.First();
            error = default;
            return true;
        }

        public static bool TryGetXrefDocument(
            this IExecutionContext context,
            string xref,
            out IDocument document,
            out string error) =>
            context.TryGetXrefDocument(xref, context.GetXrefMappings(), out document, out error);

        public static bool TryGetXrefDocument(this IExecutionContext context, string xref, out IDocument document) =>
            context.TryGetXrefDocument(xref, out document, out string _);

        public static IDocument GetXrefDocument(this IExecutionContext context, string xref) =>
            context.TryGetXrefDocument(xref, out IDocument document, out string error) ? document : throw new ExecutionException(error);

        public static bool TryGetXrefLink(
            this IExecutionContext context,
            string xref,
            bool includeHost,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            out string link,
            out string error)
        {
            if (context.TryGetXrefDocument(xref, xrefMappings, out IDocument document, out error))
            {
                link = document.GetLink(includeHost);
                return link is object;
            }
            link = default;
            return false;
        }

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, bool includeHost, out string link, out string error)
        {
            if (context.TryGetXrefDocument(xref, out IDocument document, out error))
            {
                link = document.GetLink(includeHost);
                return link is object;
            }
            link = default;
            return false;
        }

        public static bool TryGetXrefLink(
            this IExecutionContext context,
            string xref,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            out string link,
            out string error) =>
            context.TryGetXrefLink(xref, false, xrefMappings, out link, out error);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, out string link, out string error) =>
            context.TryGetXrefLink(xref, false, out link, out error);

        public static bool TryGetXrefLink(
            this IExecutionContext context,
            string xref,
            bool includeHost,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            out string link) =>
            context.TryGetXrefLink(xref, includeHost, xrefMappings, out link, out string _);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, bool includeHost, out string link) =>
            context.TryGetXrefLink(xref, includeHost, out link, out string _);

        public static bool TryGetXrefLink(
            this IExecutionContext context,
            string xref,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            out string link) =>
            context.TryGetXrefLink(xref, false, xrefMappings, out link, out string _);

        public static bool TryGetXrefLink(this IExecutionContext context, string xref, out string link) =>
            context.TryGetXrefLink(xref, false, out link, out string _);

        public static string GetXrefLink(
            this IExecutionContext context,
            string xref,
            IDictionary<string, ICollection<IDocument>> xrefMappings,
            bool includeHost = false) =>
            context.TryGetXrefLink(xref, includeHost, xrefMappings, out string link, out string error) ? link : throw new ExecutionException(error);

        public static string GetXrefLink(this IExecutionContext context, string xref, bool includeHost = false) =>
            context.TryGetXrefLink(xref, includeHost, out string link, out string error) ? link : throw new ExecutionException(error);

        public static IDictionary<string, ICollection<IDocument>> GetXrefMappings(this IExecutionContext context) =>
            context
                .Outputs
                .FromPipelines(
                    context.Settings.GetList(WebKeys.XrefPipelines, Array.Empty<string>())
                        .Concat(context.Settings.GetList(WebKeys.AdditionalXrefPipelines, Array.Empty<string>()))
                        .ToArray())
                .Flatten()
                .Select(x => (x.GetString(WebKeys.Xref), x))
                .Where(x => x.Item1 is object)
                .GroupBy(x => x.Item1, x => x.Item2, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => (ICollection<IDocument>)x.ToArray(), StringComparer.OrdinalIgnoreCase);
    }
}