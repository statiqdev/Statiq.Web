using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // This was helpful: http://weblog.west-wind.com/posts/2012/Feb/01/Dynamic-Types-and-DynamicObject-References-in-C
    public class Metadata
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

        public bool Contains(string key)
        {
            return _metadata.FirstOrDefault(x => x.ContainsKey(key)) != null;
        }

        public object Get(string key)
        {
            object value;
            if (!TryGet(key, out value))
            {
                throw new KeyNotFoundException(string.Format("The key {0} was not found in the metadata.", key));
            }
            return value;
        }

        public bool TryGet(string key, out object value)
        {
            value = null;
            IDictionary<string, object> meta = _metadata.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                return false;
            }
            value = meta[key];
            return true;
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
            get { return new DynamicMetadata(this); }
        }

        private class DynamicMetadata : DynamicObject
        {
            private readonly Metadata _metadata;

            public DynamicMetadata(Metadata metadata)
            {
                _metadata = metadata;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = _metadata.Get(binder.Name);
                return true;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                _metadata.Set(binder.Name, value);
                return true;
            }
        }
    }
}
