using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;

namespace Statiq.Web
{
    public static class IDocumentExtensions
    {
        /// <summary>
        /// Gets the published date by first checking metadata for <see cref="WebKeys.Published"/>,
        /// then checking for a file name prefix of the form "yyyy/mm/dd-" (or the locale equivalent),
        /// then using the last modified date of the file.
        /// </summary>
        /// <param name="document">The document to get the published date for.</param>
        /// <param name="useLastModifiedDate"><c>true</c> to infer published date from the file last modified date, <c>false</c> otherwise.</param>
        /// <returns>The published date of the document or <see cref="DateTime.Today"/> if a published date can't be determined.</returns>
        public static DateTime GetPublishedDate(this IDocument document, bool useLastModifiedDate = true) => document.GetPublishedDate(IExecutionContext.Current, useLastModifiedDate);

        /// <summary>
        /// Gets the published date by first checking metadata for <see cref="WebKeys.Published"/>,
        /// then checking for a file name prefix of the form "yyyy/mm/dd-" (or the locale equivalent),
        /// then using the last modified date of the file.
        /// </summary>
        /// <param name="document">The document to get the published date for.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="useLastModifiedDate"><c>true</c> to infer published date from the file last modified date, <c>false</c> otherwise.</param>
        /// <returns>The published date of the document or <see cref="DateTime.Today"/> if a published date can't be determined.</returns>
        public static DateTime GetPublishedDate(this IDocument document, IExecutionContext context, bool useLastModifiedDate = true)
        {
            document.ThrowIfNull(nameof(document));
            context.ThrowIfNull(nameof(context));

            // Check metadata
            if (document.ContainsKey(WebKeys.Published) && context.TryParseInputDateTime(document.GetString(WebKeys.Published), out DateTime metadataDate))
            {
                return metadataDate;
            }

            if (!document.Source.IsNull)
            {
                // Check filename
                if (((document.Source.FileName.FullPath.Length >= 11 && document.Source.FileName.FullPath.EndsWith('-')) || document.Source.FileName.FullPath.Length == 10)
                    && context.TryParseInputDateTime(document.Source.FileName.FullPath.Substring(0, 10), out DateTime fileNameDate))
                {
                    return fileNameDate;
                }

                // Check file modified date
                if (useLastModifiedDate)
                {
                    IFile file = context.FileSystem.GetFile(document.Source);
                    if (file is object)
                    {
                        return file.LastWriteTime;
                    }
                }
            }

            // If all else fails, use today
            return DateTime.Today;
        }
    }
}
