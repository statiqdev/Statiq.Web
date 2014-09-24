using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // This was helpful: http://weblog.west-wind.com/posts/2012/Feb/01/Dynamic-Types-and-DynamicObject-References-in-C
    // Starts with one top-level variable dictionary
    internal class MetadataStack : DynamicObject
    {
        private readonly Engine _engine;
        private readonly Stack<IDictionary<string, object>> _metadata;
        private bool _locked = false;

        public MetadataStack(Engine engine)
        {
            _engine = engine;
             _metadata = new Stack<IDictionary<string, object>>();
             _metadata.Push(new Dictionary<string, object>());
        }

        private MetadataStack(MetadataStack variableStack)
        {
            _engine = variableStack._engine;
            _metadata = new Stack<IDictionary<string, object>>(variableStack._metadata.Reverse());
            if (_metadata.Peek().Count != 0)
            {
                // Only need to push a new one if there's actually something on the top
                _metadata.Push(new Dictionary<string, object>());
            }
        }

        // This clones the stack and pushes a new dictionary on to the cloned stack
        internal MetadataStack Clone()
        {
            return new MetadataStack(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            IDictionary<string, object> meta = _metadata.FirstOrDefault(x => x.ContainsKey(binder.Name));
            if (meta != null)
            {
                result = meta[binder.Name];
                return true;
            }
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if(_locked)
            {
                return false;
            }

            if (_metadata.Any(x => x.ContainsKey(binder.Name)))
            {
                _engine.Trace.Warning("Duplicate value for metadata: {0}.", binder.Name);
            }
            _metadata.Peek()[binder.Name] = value;

            return true;
        }

        // This locks the stack so no more values can be added
        public void Lock()
        {
            _locked = true;
        }
    }
}
