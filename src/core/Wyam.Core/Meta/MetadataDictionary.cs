using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Meta;

namespace Wyam.Core.Meta
{
    internal class MetadataDictionary : Metadata, IMetadataDictionary
    {
        public MetadataDictionary()
        {
            Stack.Push(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
        }

        public MetadataDictionary(IDictionary<string, object> initialValues)
        {
            Stack.Push(new Dictionary<string, object>(initialValues, StringComparer.OrdinalIgnoreCase));
        }

        public void Add(KeyValuePair<string, object> item) => Stack.Peek().Add(item);

        public void Clear() => Stack.Peek().Clear();

        public bool Contains(KeyValuePair<string, object> item) => Stack.Peek().Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => Stack.Peek().CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, object> item) => Stack.Peek().Remove(item);

        public bool IsReadOnly { get; } = false;

        public void Add(string key, object value) => Stack.Peek().Add(key, value);

        public bool Remove(string key) => Stack.Peek().Remove(key);

        object IDictionary<string, object>.this[string key]
        {
            get { return Stack.Peek()[key]; }
            set { Stack.Peek()[key] = value; }
        }

        public new object this[string key]
        {
            get { return ((IDictionary<string, object>)this)[key]; }
            set { ((IDictionary<string, object>)this)[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys => Stack.Peek().Keys;

        ICollection<object> IDictionary<string, object>.Values => Stack.Peek().Values;

        ICollection<string> IMetadataDictionary.Keys => ((IDictionary<string, object>)this).Keys;

        ICollection<object> IMetadataDictionary.Values => ((IDictionary<string, object>)this).Values;
    }
}