using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class GroupBy : IModule
    {
        private readonly DocumentConfig _key;
        private readonly IModule[] _modules;

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
                return groupings.Select(x => input.Clone(
                    new Dictionary<string, object>
                    {
                        {MetadataKeys.GroupDocuments, x.ToImmutableArray()},
                        {MetadataKeys.GroupKey, x.Key}
                    })
                );
            });
        }
    }
}
