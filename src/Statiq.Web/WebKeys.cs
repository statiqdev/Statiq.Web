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
        // Intended for use as global or document settings

        public const string Title = nameof(Title);

        public const string Description = nameof(Description);

        public const string Author = nameof(Author);

        public const string Image = nameof(Image);

        public const string Copyright = nameof(Copyright);

        public const string OptimizeContentFileNames = nameof(OptimizeContentFileNames);

        public const string OptimizeDataFileNames = nameof(OptimizeDataFileNames);

        /// <summary>
        /// The date the file or post was published.
        /// </summary>
        /// <remarks>
        /// If you want to use a different metadata key to represent published dates you can
        /// globally fetch a value from a different key by setting <see cref="Published"/>
        /// in settings to an evaluated metadata script like <c>=> SomeOtherKey</c>.
        /// </remarks>
        public const string Published = nameof(Published);

        public const string Updated = nameof(Updated);

        public const string MirrorResources = nameof(MirrorResources);

        // Intended for use as document metadata

        public const string ArchivePipelines = nameof(ArchivePipelines);

        public const string ArchiveSources = nameof(ArchiveSources);

        public const string ArchiveFilter = nameof(ArchiveFilter);

        public const string ArchiveKey = nameof(ArchiveKey);

        public const string ArchivePageSize = nameof(ArchivePageSize);

        public const string ArchiveTitle = nameof(ArchiveTitle);

        public const string ArchiveDestination = nameof(ArchiveDestination);

        public const string ArchiveOrderKey = nameof(ArchiveOrderKey);  // The key that should be sorted

        public const string ArchiveOrderDescending = nameof(ArchiveOrderDescending);  // Indicates the archive should be sorted in descending order

        public const string FeedPipelines = nameof(FeedPipelines);

        public const string FeedSources = nameof(FeedSources);

        public const string FeedFilter = nameof(FeedFilter);

        public const string FeedOrderKey = nameof(FeedOrderKey);  // The key that should be sorted

        public const string FeedOrderDescending = nameof(FeedOrderDescending);  // Indicates the archive should be sorted in descending order

        public const string FeedSize = nameof(FeedSize);  // The number of items the feed should contain after sorting

        public const string FeedRss = nameof(FeedRss);

        public const string FeedAtom = nameof(FeedAtom);

        public const string FeedRdf = nameof(FeedRdf);

        public const string FeedId = nameof(FeedId);  // A Uri, links to the root of the site by default

        public const string FeedTitle = nameof(FeedTitle);  // Defaults to WebKeys.Title

        public const string FeedDescription = nameof(FeedDescription);  // Defaults to WebKeys.Description

        public const string FeedAuthor = nameof(FeedAuthor);

        public const string FeedPublished = nameof(FeedPublished);

        public const string FeedUpdated = nameof(FeedUpdated);

        public const string FeedLink = nameof(FeedLink); // Uri

        public const string FeedImageLink = nameof(FeedImageLink); // Uri

        public const string FeedItemId = nameof(FeedItemId);

        public const string FeedItemTitle = nameof(FeedItemTitle);

        public const string FeedItemDescription = nameof(FeedItemDescription);

        public const string FeedItemAuthor = nameof(FeedItemAuthor);

        public const string FeedItemPublished = nameof(FeedItemPublished);

        public const string FeedItemUpdated = nameof(FeedItemUpdated);

        public const string FeedItemLink = nameof(FeedItemLink);

        public const string FeedItemImageLink = nameof(FeedItemImageLink);

        public const string FeedItemContent = nameof(FeedItemContent);

        public const string FeedItemThreadLink = nameof(FeedItemThreadLink);

        public const string FeedItemThreadCount = nameof(FeedItemThreadCount);

        public const string FeedItemThreadUpdated = nameof(FeedItemThreadUpdated);

        /// <summary>
        /// Indicates that the data file (.json, .yaml, etc.) should be output (by defaut data files are not output).
        /// </summary>
        public const string OutputData = nameof(OutputData);

        /// <summary>
        /// Indicates the layout file that should be used for this document.
        /// </summary>
        public const string Layout = nameof(Layout);
    }
}
