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
        public static DateTime GetPublishedDate(this IDocument document, IExecutionContext context)
        {
            _ = document ?? throw new ArgumentNullException(nameof(document));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            // Check metadata
            if (document.ContainsKey(WebKeys.Published) && context.TryParseInputDateTime(document.GetString(WebKeys.Published), out DateTime metadataDate))
            {
                return metadataDate;
            }

            if (document.Source != null)
            {
                // Check filename
                if (((document.Source.FileName.FullPath.Length >= 11 && document.Source.FileName.FullPath.EndsWith('-')) || document.Source.FileName.FullPath.Length == 10)
                    && context.TryParseInputDateTime(document.Source.FileName.FullPath.Substring(0, 10), out DateTime fileNameDate))
                {
                    return fileNameDate;
                }

                // Check file modified date
                IFile file = context.FileSystem.GetFile(document.Source);
                if (file != null)
                {
                    return file.LastWriteTime;
                }
            }

            // If all else fails, use today
            return DateTime.Today;
        }
    }
}
