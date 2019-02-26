using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Html
{
    /// <summary>
    /// Queries HTML content of the input documents and adds a metadata value that contains it's headings.
    /// </summary>
    /// <remarks>
    /// A new document is created for each heading, all of which are placed into a <c>IReadOnlyList&lt;IDocument&gt;</c>
    /// in the metadata of each input document. The new heading documents contain metadata with the level of the heading,
    /// the children of the heading (the following headings with one deeper level) and optionally the heading content, which
    /// is also set as the content of each document. The output of this module is the input documents with the additional
    /// metadata value containing the documents that present each heading.
    /// </remarks>
    /// <metadata cref="HtmlKeys.Headings" usage="Output"/>
    /// <metadata cref="HtmlKeys.Level" usage="Output"/>
    /// <metadata cref="HtmlKeys.Id" usage="Output"/>
    /// <metadata cref="Keys.Children" usage="Output">
    /// The child heading documents of the current heading document.
    /// </metadata>
    /// <metadata cref="Keys.Parent" usage="Output">
    /// The parent heading document of the current heading document.
    /// </metadata>
    /// <category>Metadata</category>
    public class Headings : IModule
    {
        private int _level;
        private bool _nesting;
        private string _metadataKey = HtmlKeys.Headings;
        private string _levelKey = HtmlKeys.Level;
        private string _idKey = HtmlKeys.Id;
        private string _childrenKey = Keys.Children;
        private string _parentKey = Keys.Parent;
        private string _headingKey;

        public Headings()
            : this(1)
        {
        }

        public Headings(int level)
        {
            WithLevel(level);
        }

        /// <summary>
        /// Sets the deepest heading level to get. The default is to
        /// only query for top-level headings (level 1).
        /// </summary>
        /// <param name="level">The deepest heading level to get.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentException("Level cannot be less than 1");
            }
            if (level > 6)
            {
                throw new ArgumentException("Level cannot be greater than 6");
            }
            _level = level;

            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the level.
        /// </summary>
        /// <param name="levelKey">The key to use for the level.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithLevelKey(string levelKey)
        {
            _levelKey = levelKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the heading
        /// <c>id</c> attribute (if it has one).
        /// </summary>
        /// <param name="idKey">The key to use for the <c>id</c>.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithIdKey(string idKey)
        {
            _idKey = idKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the children
        /// of a given heading. In other words, the metadata for this key will
        /// contain all the headings following the one in the document with a
        /// level one deeper than the current heading.
        /// </summary>
        /// <param name="childrenKey">The key to use for children.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithChildrenKey(string childrenKey)
        {
            _childrenKey = childrenKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the parent
        /// of a given heading.
        /// </summary>
        /// <param name="parentKey">The key to use for the parent.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithParentKey(string parentKey)
        {
            _parentKey = parentKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use for storing the heading content in the heading documents.
        /// The default is <c>null</c> which means only store the heading content in the
        /// content of the heading document. Setting this can be useful when you want
        /// to use the heading documents in downstream modules, setting their content
        /// to something else while maintaining the heading content in metadata.
        /// </summary>
        /// <param name="headingKey">The key to use for the heading content.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithHeadingKey(string headingKey)
        {
            _headingKey = headingKey;
            return this;
        }

        /// <summary>
        /// Controls whether the heading documents are nested. If nesting is
        /// used, only the level 1 headings will be in the root set of documents.
        /// The rest of the heading documents will only be accessible via the
        /// metadata of the root heading documents.
        /// </summary>
        /// <param name="nesting"><c>true</c> to turn on nesting</param>
        /// <returns>The current module instance.</returns>
        public Headings WithNesting(bool nesting = true)
        {
            _nesting = true;
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate metadata key for the heading documents.
        /// </summary>
        /// <param name="metadataKey">The metadata key to store the heading documents in.</param>
        /// <returns>The current module instance.</returns>
        public Headings WithMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<Common.Documents.IDocument> Execute(IReadOnlyList<Common.Documents.IDocument> inputs, IExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(_metadataKey))
            {
                return inputs;
            }

            // Build the query
            StringBuilder query = new StringBuilder();
            for (int level = 1; level <= _level; level++)
            {
                if (level > 1)
                {
                    query.Append(",");
                }
                query.Append("h");
                query.Append(level);
            }

            // Process documents
            HtmlParser parser = new HtmlParser();
            return inputs.AsParallel().Select(context, input =>
            {
                // Parse the HTML content
                IHtmlDocument htmlDocument = input.ParseHtml(parser);
                if (htmlDocument == null)
                {
                    return input;
                }

                // Evaluate the query and create the holding nodes
                Heading previousHeading = null;
                List<Heading> headings = htmlDocument
                    .QuerySelectorAll(query.ToString())
                    .Select(x =>
                    {
                        previousHeading = new Heading
                        {
                            Element = x,
                            Previous = previousHeading,
                            Level = int.Parse(x.NodeName.Substring(1))
                        };
                        return previousHeading;
                    })
                    .ToList();

                // Build the tree from the bottom-up
                for (int level = _level; level >= 1; level--)
                {
                    int currentLevel = level;
                    foreach (Heading heading in headings.Where(x => x.Level == currentLevel))
                    {
                        // Get the parent
                        Heading parent = null;
                        if (currentLevel > 1)
                        {
                            parent = heading.Previous;
                            while (parent != null && parent.Level >= currentLevel)
                            {
                                parent = parent.Previous;
                            }
                        }

                        // Create the document
                        MetadataItems metadata = new MetadataItems();
                        if (_levelKey != null)
                        {
                            metadata.Add(_levelKey, heading.Level);
                        }
                        if (_idKey != null && heading.Element.HasAttribute("id"))
                        {
                            metadata.Add(_idKey, heading.Element.GetAttribute("id"));
                        }
                        if (_headingKey != null)
                        {
                            metadata.Add(_headingKey, heading.Element.InnerHtml);
                        }
                        if (_childrenKey != null)
                        {
                            metadata.Add(_childrenKey, heading.Children.AsReadOnly());
                        }
                        if (_parentKey != null)
                        {
                            metadata.Add(_parentKey, new CachedDelegateMetadataValue(_ => parent?.Document));
                        }
                        Stream contentStream = context.GetContentStream();
                        using (StreamWriter writer = contentStream.GetWriter())
                        {
                            heading.Element.ChildNodes.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                            writer.Flush();
                            heading.Document = context.GetDocument(contentStream, metadata);
                        }

                        // Add to parent
                        parent?.Children.Add(heading.Document);
                    }
                }

                return context.GetDocument(
                    input,
                    new MetadataItems
                    {
                        {
                            _metadataKey,
                            _nesting
                                ? headings
                                    .Where(x => x.Level == headings.Min(y => y.Level))
                                    .Select(x => x.Document)
                                    .ToArray()
                                : headings
                                    .Select(x => x.Document)
                                    .ToArray()
                        }
                    });
            });
        }

        private class Heading
        {
            public IElement Element { get; set; }
            public Heading Previous { get; set; }
            public int Level { get; set; }
            public Common.Documents.IDocument Document { get; set; }
            public List<Common.Documents.IDocument> Children { get; } = new List<Common.Documents.IDocument>();
        }
    }
}