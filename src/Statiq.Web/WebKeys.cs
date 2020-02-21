using System;
using System.Collections.Generic;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web
{
    public static class WebKeys
    {
        // Intended for use as global settings
        public const string MirrorResources = nameof(MirrorResources);

        // Intended for use as document metadata

        public const string ArchivePipelines = nameof(ArchivePipelines);

        public const string ArchiveSources = nameof(ArchiveSources);

        public const string ArchiveFilter = nameof(ArchiveFilter);

        public const string ArchiveKey = nameof(ArchiveKey);

        public const string ArchivePageSize = nameof(ArchivePageSize);

        public const string ArchiveTitle = nameof(ArchiveTitle);

        public const string ArchiveDestination = nameof(ArchiveDestination);

        public const string ArchiveOrder = nameof(ArchiveOrder);  // The key that should be sorted (or group key by default)

        public const string ArchiveDescending = nameof(ArchiveDescending);  // Indicates the archive should be sorted in descending order

        /// <summary>
        /// Indicates the layout file that should be used for this document.
        /// </summary>
        public const string Layout = nameof(Layout);

        /// <summary>
        /// The date the file or post was published.
        /// </summary>
        /// <remarks>
        /// If you want to use a different metadata key to represent published dates you can
        /// globally fetch a value from a different key by setting <see cref="Published"/>
        /// in settings to an evaluated metadata script like <c>=> SomeOtherKey</c>.
        /// </remarks>
        public const string Published = nameof(Published);
    }
}
