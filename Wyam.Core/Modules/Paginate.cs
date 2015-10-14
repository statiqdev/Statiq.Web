using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class Paginate : IModule
    {
        private readonly int _pageSize;
        private readonly IModule[] _modules;

        public Paginate(int pageSize, params IModule[] modules)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }
            _pageSize = pageSize;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<ImmutableArray<IDocument>> partitions 
                = Partition(context.Execute(_modules, inputs), _pageSize).ToImmutableArray();
            if (partitions.Length == 0)
            {
                return inputs;
            }
            return inputs.SelectMany(input =>
            {
                return partitions.Select((x, i) => input.Clone(
                    new Dictionary<string, object>
                    {
                        {MetadataKeys.PageDocuments, partitions[i]},
                        {MetadataKeys.CurrentPage, i + 1},
                        {MetadataKeys.TotalPages, partitions.Length},
                        {MetadataKeys.HasNextPage, partitions.Length > i + 1},
                        {MetadataKeys.HasPreviousPage, i != 0}
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
