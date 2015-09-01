using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
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
            List<IEnumerable<IDocument>> partitions = Partition(inputs, _pageSize).ToList();
            for(int c = 0 ; c < partitions.Count ; c++)
            {
                foreach (IDocument result in context.Execute(_modules, 
                    new Dictionary<string, object>
                    {
                        { MetadataKeys.CurrentPage, c + 1 },
                        { MetadataKeys.TotalPages, partitions.Count },
                        { MetadataKeys.HasNextPage, partitions.Count > c + 1 },
                        { MetadataKeys.HasPreviousPage, c != 0 }
                    }))
                {
                    yield return result;
                }
            }
        }

        // Interesting discussion of partitioning at
        // http://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        // Note that this implementation won't work for very long sequences because it enumerates twice per chunk
        private static IEnumerable<IEnumerable<T>> Partition<T>(IReadOnlyList<T> source, int size)
        {
            int pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(size);
                pos += size;
            }
        }
    }
}
