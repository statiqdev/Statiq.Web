using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Splits a sequence of documents into multiple pages.
    /// </summary>
    /// <remarks>
    /// This module forms pages from the output documents of the specified modules.
    /// Each input document is cloned for each page and metadata related 
    /// to the pages, including the sequence of documents for each page, 
    /// is added to each clone. For example, if you have 2 input documents
    /// and the result of paging is 3 pages, this module will output 6 documents.
    /// </remarks>
    /// <example>
    /// If your input document is a Razor template for a blog archive, you can use 
    /// Paginate to get pages of 10 blog posts each. If you have 50 blog posts, the 
    /// result of the Paginate module will be 5 copies of your input archive template, 
    /// one for each page. Your configuration file might look something like this:
    /// <code>
    /// Pipelines.Add("Posts",
    ///     ReadFiles("*.md"),
    ///     Markdown(),
    ///     WriteFiles("html")
    /// );
    ///
    /// Pipelines.Add("Archive",
    ///     ReadFiles("archive.cshtml"),
    ///     Paginate(10,
    ///         Documents("Posts")
    ///     ),
    ///     Razor(),
    ///     WriteFiles(string.Format("archive-{0}.html", @doc["CurrentPage"]))
    /// );
    /// </code>
    /// </example>
    /// <metadata name="PageDocuments" type="IEnumerable&lt;IDocument&gt;">Contains all the documents for the current page.</metadata>
    /// <metadata name="CurrentPage" type="int">The index of the current page (1 based).</metadata>
    /// <metadata name="TotalPages" type="int">The total number of pages.</metadata>
    /// <metadata name="HasNextPage" type="bool">Whether there is another page after this one.</metadata>
    /// <metadata name="HasPreviousPage" type="bool">Whether there is another page before this one.</metadata>
    /// <category>Control</category>
    public class Paginate : IModule
    {
        private readonly int _pageSize;
        private readonly IModule[] _modules;
        private Func<IDocument, IExecutionContext, bool> _predicate;

        /// <summary>
        /// Partitions the result of the specified modules into the specified number of pages. The 
        /// input documents to Paginate are used as the initial input documents to the specified modules.
        /// </summary>
        /// <param name="pageSize">The number of documents on each page.</param>
        /// <param name="modules">The modules to execute to get the documents to page.</param>
        /// <exception cref="System.ArgumentException"></exception>
        public Paginate(int pageSize, params IModule[] modules)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }

            _pageSize = pageSize;
            _modules = modules;
        }

        /// <summary>
        /// Limits the documents to be paged to those that satisfy the supplied predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        public Paginate Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>)
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<ImmutableArray<IDocument>> partitions 
                = Partition(
                    context.Execute(_modules, inputs).Where(x => _predicate?.Invoke(x, context) ?? true).ToList(),
                    _pageSize)
                    .ToImmutableArray();
            if (partitions.Length == 0)
            {
                return inputs;
            }
            return inputs.SelectMany(input =>
            {
                return partitions.Select((x, i) => context.GetDocument(input,
                    new Dictionary<string, object>
                    {
                        {Keys.PageDocuments, partitions[i]},
                        {Keys.CurrentPage, i + 1},
                        {Keys.TotalPages, partitions.Length},
                        {Keys.HasNextPage, partitions.Length > i + 1},
                        {Keys.HasPreviousPage, i != 0}
                    })
                );
            });
        }

        // Interesting discussion of partitioning at
        // http://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        // Note that this implementation won't work for very long sequences because it enumerates twice per chunk
        private static IEnumerable<ImmutableArray<T>> Partition<T>(IReadOnlyList<T> source, int size)
        {
            int pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(size).ToImmutableArray();
                pos += size;
            }
        }
    }
}
