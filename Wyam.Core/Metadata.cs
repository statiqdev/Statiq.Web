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
    public class Metadata : DynamicObject
    {
        private readonly Engine _engine;
        private readonly Stack<IDictionary<string, object>> _metadata;
        
        internal Metadata(Engine engine)
        {
            _engine = engine;
             _metadata = new Stack<IDictionary<string, object>>();
             _metadata.Push(new Dictionary<string, object>());
        }

        private Metadata(Metadata variableStack)
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
        internal Metadata Clone()
        {
            return new Metadata(this);
        }

        // This locks the stack so no more values can be added
        internal bool IsReadOnly { get; set; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = Get(binder.Name);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Set(binder.Name, value);
            return true;
        }

        public object Get(string key)
        {
            IDictionary<string, object> meta = _metadata.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                throw new KeyNotFoundException(string.Format("The key {0} was not found in the metadata.", key));
            }
            return meta[key];
        }

        public void Set(string key, object value)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(string.Format("The metadata is read-only and the key {0} can not be set.", key));
            }

            if (_metadata.Any(x => x.ContainsKey(key)))
            {
                _engine.Trace.Warning("Existing value found while setting metadata key {0}.", value);
            }

            _metadata.Peek()[key] = value;
        }

        // A little syntactic sugar for the dynamic cast
        public dynamic Dynamic
        {
            get { return this; }
        }
    }
}
