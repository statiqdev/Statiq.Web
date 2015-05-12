using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core
{
    // This class contains a stack of all the metadata generated at a particular pipeline stage
    // Getting a value checks each of the stacks and returns the first hit
    // This class is immutable, use IDocument.Clone() to get a new one with additional values
    internal class Metadata : IMetadata
    {
        private readonly Engine _engine;
        private readonly Stack<IDictionary<string, object>> _metadataStack;
        
        internal Metadata(Engine engine)
        {
            _engine = engine;
            _metadataStack = new Stack<IDictionary<string, object>>();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in engine.Metadata)
            {
                dictionary[item.Key] = item.Value;
            }
            _metadataStack.Push(dictionary);
        }

        private Metadata(Metadata variableStack, IEnumerable<KeyValuePair<string, object>> items)
        {
            _engine = variableStack._engine;
            _metadataStack = new Stack<IDictionary<string, object>>(variableStack._metadataStack.Reverse());
            _metadataStack.Push(new Dictionary<string, object>());

            // Set new items
            if (items != null)
            {
                foreach (KeyValuePair<string, object> item in items)
                {
                    if (_metadataStack.Any(x => x.ContainsKey(item.Key)))
                    {
                        _engine.Trace.Warning("Existing value found while setting metadata key {0}.", item.Key);
                    }

                    _metadataStack.Peek()[item.Key] = item.Value;
                }
            }
        }

        // This clones the stack and pushes a new dictionary on to the cloned stack
        internal Metadata Clone(IEnumerable<KeyValuePair<string, object>> items)
        {
            return new Metadata(this, items);
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return _metadataStack.FirstOrDefault(x => x.ContainsKey(key)) != null;
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            value = null;
            IDictionary<string, object> meta = _metadataStack.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                return false;
            }
            value = meta[key];
            return true;
        }

        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                object value;
                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException();
                }
                return value;
            }
        }

        public IEnumerable<string> Keys
        {
            get { return _metadataStack.SelectMany(x => x.Keys); }
        }

        public IEnumerable<object> Values
        {
            get { return _metadataStack.SelectMany(x => x.Values); }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadataStack.SelectMany(x => x).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _metadataStack.Sum(x => x.Count); }
        }
    }
}
