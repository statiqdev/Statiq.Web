using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Modules
{
    public class Metadata : IModule
    {
        private readonly string _key;
        private readonly Func<IModuleContext, object> _metadata;

        public Metadata(string key, object metadata)
            : this(key, x => metadata)
        {
        }

        public Metadata(string key, Func<IModuleContext, object> metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _key = key;
            _metadata = metadata;
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => _metadata == null ? x : x.Clone(new [] { new KeyValuePair<string, object>(_key, _metadata(x)) }));
        }
    }
}
