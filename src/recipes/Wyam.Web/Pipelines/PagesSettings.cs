using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Core.Modules.IO;

namespace Wyam.Web.Pipelines
{
    /// <summary>
    /// Settings for the <see cref="Pages"/> pipeline.
    /// </summary>
    public class PagesSettings
    {
        /// <summary>
        /// A delegate that should return a <see cref="string"/> with the glob to pages.
        /// If <c>null</c>, a default globbing pattern of "**" is used. This should match
        /// one or more directories.
        /// </summary>
        public ContextConfig PagesPattern { get; set; }

        /// <summary>
        /// A delegate that should return a <see cref="string"/>
        /// or <c>IEnumerable&lt;string&gt;</c> with patterns for folders and files to ignore.
        /// If the delegate is <c>null</c>, no paths will be ignored.
        /// </summary>
        public ContextConfig IgnorePaths { get; set; }

        /// <summary>
        /// A delegate that returns the string configuration for the Markdown processor.
        /// </summary>
        public ContextConfig MarkdownConfiguration { get; set; } = _ => Markdown.Markdown.DefaultConfiguration;

        /// <summary>
        /// A delegate that returns a sequence of <see cref="Type"/> for Markdown extensions.
        /// </summary>
        public ContextConfig MarkdownExtensionTypes { get; set; } = _ => null;

        /// <summary>
        /// A delegate that returns a <see cref="bool"/> indicating if documents should be processed with the <see cref="Include"/> module.
        /// </summary>
        public DocumentConfig ProcessIncludes { get; set; } = (doc, ctx) => false;

        /// <summary>
        /// Sorts the documents based on a comparison. If <c>null</c>, the sorting will be based on the document title.
        /// </summary>
        public Comparison<IDocument> Sort { get; set; }

        /// <summary>
        /// <c>true</c> to create a tree from the pages, <c>false</c> to leave the pages flat.
        /// </summary>
        public bool CreateTree { get; set; }

        /// <summary>
        /// A factory to use for creating tree placeholders at points in the tree where no actual pages were found.
        /// If <c>null</c>, the default placeholder factory will be used which outputs empty index files.
        /// </summary>
        public Func<object[], MetadataItems, IExecutionContext, IDocument> TreePlaceholderFactory { get; set; }

        /// <summary>
        /// Set to <c>true</c> to prepend a configured <c>LinkRoot</c> to all root-relative Markdown links.
        /// </summary>
        public ContextConfig PrependLinkRoot { get; set; }
    }
}
