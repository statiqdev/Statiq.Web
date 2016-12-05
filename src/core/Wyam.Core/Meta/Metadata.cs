using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Core.Meta
{
    internal abstract class Metadata : IMetadata
    {
        protected internal Stack<IDictionary<string, object>> Stack { get; }

        protected Metadata()
        {
            Stack = new Stack<IDictionary<string, object>>();
        }

        protected Metadata(Stack<IDictionary<string, object>> stack)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            Stack = stack;
        }

        public IMetadata<T> MetadataAs<T>() => new MetadataAs<T>(this);

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return Stack.FirstOrDefault(x => x.ContainsKey(key)) != null;
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            value = null;
            IDictionary<string, object> meta = Stack.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                return false;
            }
            value = GetValue(meta[key]);
            return true;
        }

        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public object GetRaw(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            IDictionary<string, object> meta = Stack.FirstOrDefault(x => x.ContainsKey(key));
            if (meta == null)
            {
                throw new KeyNotFoundException(nameof(key));
            }
            return meta[key];
        }

        public T Get<T>(string key) => MetadataAs<T>().Get(key);

        public T Get<T>(string key, T defaultValue) => MetadataAs<T>().Get(key, defaultValue);

        public string String(string key, string defaultValue = null) => Get(key, defaultValue);

        public FilePath FilePath(string key, FilePath defaultValue = null) => Get(key, defaultValue);

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null) => Get(key, defaultValue);

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => Get<IReadOnlyList<T>>(key, defaultValue);

        public IDocument Document(string key) => Get<IDocument>(key);

        public IReadOnlyList<IDocument> DocumentList(string key) => Get<IReadOnlyList<IDocument>>(key);

        public dynamic Dynamic(string key, object defaultValue = null) => Get(key, defaultValue) ?? defaultValue;

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                object value;
                if (!TryGetValue(key, out value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => Stack.SelectMany(x => x.Keys);

        public IEnumerable<object> Values => Stack.SelectMany(x => x.Select(y => GetValue(y.Value)));

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            Stack.SelectMany(x => x.Select(GetItem)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Stack.Sum(x => x.Count);
        
        /// <summary>
        /// This resolves the metadata value by recursively expanding IMetadataValue.
        /// </summary>
        private object GetValue(object originalValue)
        {
            IMetadataValue metadataValue = originalValue as IMetadataValue;
            return metadataValue != null ? GetValue(metadataValue.Get(this)) : originalValue;
        }
        
        /// <summary>
        /// This resolves the metadata value by expanding IMetadataValue.
        /// </summary>
        private KeyValuePair<string, object> GetItem(KeyValuePair<string, object> item)
        {
            IMetadataValue metadataValue = item.Value as IMetadataValue;
            return metadataValue != null ? new KeyValuePair<string, object>(item.Key, GetValue(metadataValue.Get(this))) : item;
        }
    }
}