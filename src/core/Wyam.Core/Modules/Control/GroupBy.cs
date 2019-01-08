using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Splits a sequence of documents into groups based on a specified function or metadata key.
    /// </summary>
    /// <remarks>
    /// This module forms groups from the output documents of the specified modules.
    /// Each input document is cloned for each group and metadata related
    /// to the groups, including the sequence of documents for each group,
    /// is added to each clone. For example, if you have 2 input documents
    /// and the result of grouping is 3 groups, this module will output 6 documents.
    /// </remarks>
    /// <metadata cref="Keys.GroupDocuments" usage="Output" />
    /// <metadata cref="Keys.GroupKey" usage="Output" />
    /// <category>Control</category>
    public class GroupBy : ContainerModule
    {
        private readonly DocumentConfig _key;
        private Func<IDocument, IExecutionContext, bool> _predicate;
        private IEqualityComparer<object> _comparer;
        private bool _emptyOutputIfNoGroups;

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the key delegate.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="key">A delegate that returns the group key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupBy(DocumentConfig key, params IModule[] modules)
            : this(key, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the key delegate.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="key">A delegate that returns the group key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupBy(DocumentConfig key, IEnumerable<IModule> modules)
            : base(modules)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the value at the specified metadata key.
        /// If a document to group does not contain the specified metadata key, it is not included in any output groups.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupBy(string keyMetadataKey, params IModule[] modules)
            : this(keyMetadataKey, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys
        /// based on the value at the specified metadata key.
        /// If a document to group does not contain the specified metadata key, it is not included in any output groups.
        /// The input documents to GroupBy are used as
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="keyMetadataKey">The key metadata key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupBy(string keyMetadataKey, IEnumerable<IModule> modules)
            : base(modules)
        {
            if (keyMetadataKey == null)
            {
                throw new ArgumentNullException(nameof(keyMetadataKey));
            }

            _key = (doc, ctx) => doc.Get(keyMetadataKey);
            _predicate = (doc, ctx) => doc.ContainsKey(keyMetadataKey);
        }

        /// <summary>
        /// Limits the documents to be grouped to those that satisfy the supplied predicate.
        /// </summary>
        /// <param name="predicate">A delegate that should return a <c>bool</c>.</param>
        /// <returns>The current module instance.</returns>
        public GroupBy Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)predicate.Invoke<bool>
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        /// <summary>
        /// Specifies an equality comparer to use for the grouping.
        /// </summary>
        /// <param name="comparer">The equality comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public GroupBy WithComparer(IEqualityComparer<object> comparer)
        {
            _comparer = comparer;
            return this;
        }

        /// <summary>
        /// Specifies a typed equality comparer to use for the grouping. A conversion to the
        /// comparer type will be attempted for all metadata values. If the conversion fails,
        /// the value will not be considered equal. Note that this will also have the effect
        /// of treating different convertible types as being of the same type. For example,
        /// if you have two group keys, 1 and "1" (in that order), and use a string-based comparison, you will
        /// only end up with a single group for those documents with a group key of 1 (since the <c>int</c> key came first).
        /// </summary>
        /// <param name="comparer">The typed equality comparer to use.</param>
        /// <returns>The current module instance.</returns>
        public GroupBy WithComparer<TValue>(IEqualityComparer<TValue> comparer)
        {
            _comparer = comparer == null ? null : new ConvertingEqualityComparer<TValue>(comparer);
            return this;
        }

        /// <summary>
        /// Specifies that no documents should be output if there are no groups. This is in contrast to the
        /// default behavior of outputting the unmodified input documents if no groups were found.
        /// </summary>
        /// <param name="emptyOutput"><c>true</c> to not output documents when no groups are found.</param>
        /// <returns>The current module instance.</returns>
        public GroupBy WithEmptyOutputIfNoGroups(bool emptyOutput = true)
        {
            _emptyOutputIfNoGroups = emptyOutput;
            return this;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<IGrouping<object, IDocument>> groupings = context.Execute(this, inputs)
                .Where(context, x => _predicate?.Invoke(x, context) ?? true)
                .GroupBy(x => _key(x, context), _comparer)
                .ToImmutableArray();
            if (groupings.Length == 0)
            {
                return _emptyOutputIfNoGroups ? Array.Empty<IDocument>() : inputs;
            }
            return inputs.SelectMany(context, input =>
            {
                return groupings.Select(x => context.GetDocument(
                    input,
                    new MetadataItems
                    {
                        { Common.Meta.Keys.GroupDocuments, x.ToImmutableArray() },
                        { Common.Meta.Keys.GroupKey, x.Key }
                    }));
            });
        }
    }
}
