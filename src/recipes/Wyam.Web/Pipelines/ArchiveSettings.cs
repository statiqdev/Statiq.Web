using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="Archive"/> pipeline.
    /// </summary>
    public class ArchiveSettings
    {
        /// <summary>
        /// The name of the pipeline(s) that contains the posts.
        /// </summary>
        public string[] Pipelines { get; set; }

        /// <summary>
        /// The relative path to the index file template.
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The layout to use for each index file, if <c>null</c> no explicit layout is specified.
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// A delegate to use for grouping documents or <c>null</c> if no grouping should be performed.
        /// </summary>
        public DocumentConfig Group { get; set; }

        /// <summary>
        /// A delegate that should return <c>true</c> the use the case-insensitive group key comparer,
        /// <c>false</c> or <c>null</c> to use the default comparer (including for non-string group keys).
        /// </summary>
        public ContextConfig CaseInsensitiveGroupComparer { get; set; }

        /// <summary>
        /// A delegate to get the page size. If <c>null</c>, no paging will be used.
        /// </summary>
        public ContextConfig PageSize { get; set; }

        /// <summary>
        /// Sorts the documents before generating the archive pages. If <c>null</c> the documents will maintain the order of their source pipeline(s).
        /// </summary>
        public Comparison<IDocument> Sort { get; set; }

        /// <summary>
        /// A delegate to get the title of each page.
        /// </summary>
        public DocumentConfig Title { get; set; }

        /// <summary>
        /// A delegate to get the relative output path of each page. If the result contains a ".html" extension, the page
        /// number will be appended to the result file name, otherwise if no ".html" extension is in the result value then
        /// it will be considered a folder path and the first page will be output as "index.html" followed by "page2.html", etc.
        /// </summary>
        public DocumentConfig RelativePath { get; set; }

        /// <summary>
        /// An additional metadata key to store the group documents in, or <c>null</c> not to store them.
        /// </summary>
        public string GroupDocumentsMetadataKey { get; set; }

        /// <summary>
        /// A metadata key to store the group key in, or <c>null</c> not to store it.
        /// </summary>
        public string GroupKeyMetadataKey { get; set; }

        /// <summary>
        /// If <c>true</c> the archive index page will be written, even if it's empty. The default behavior
        /// is to suppress writing empty pages (though they're still rendered).
        /// </summary>
        public bool WriteIfEmpty { get; set; }

        /// <summary>
        /// A delegate that specifies a number of pages to take. Must return an <c>int</c>.
        /// </summary>
        public ContextConfig TakePages { get; set; }

        /// <summary>
        /// A delegate that specifies a number of pages to skip. Must return an <c>int</c>.
        /// </summary>
        public ContextConfig SkipPages { get; set; }
    }
}
