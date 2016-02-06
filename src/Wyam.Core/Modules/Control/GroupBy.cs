using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Splits a sequence of documents into groups based on a specified key function.
    /// </summary>
    /// <remarks>
    /// This module works similarly to <see cref="Paginate"/>.
    /// </remarks>
    /// <metadata name="GroupDocuments" type="IEnumerable&lt;IDocument&gt;">Contains all the documents for the current group.</metadata>
    /// <metadata name="GroupKey" type="object">The key for the current group.</metadata>
    /// <category>Control</category>
    public class GroupBy : IModule
    {
        private readonly DocumentConfig _key;
        private readonly IModule[] _modules;

        /// <summary>
        /// Partitions the result of the specified modules into groups with matching keys 
        /// based on the key delegate. The input documents to GroupBy are used as 
        /// the initial input documents to the specified modules.
        /// </summary>
        /// <param name="key">A delegate that returns the group key.</param>
        /// <param name="modules">Modules to execute on the input documents prior to grouping.</param>
        public GroupBy(DocumentConfig key, params IModule[] modules)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<IGrouping<object, IDocument>> groupings
                = context.Execute(_modules, inputs).GroupBy(x => _key(x, context)).ToImmutableArray();
            if (groupings.Length == 0)
            {
                return inputs;
            }
            return inputs.SelectMany(input =>
            {
                return groupings.Select(x => context.GetDocument(input,
                    new MetadataItems
                    {
                        {Keys.GroupDocuments, x.ToImmutableArray()},
                        {Keys.GroupKey, x.Key}
                    })
                );
            });
        }
    }
}
