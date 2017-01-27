using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Testing.Configuration
{
    public class Settings : ISettings
    {
        private readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_metadata.TryGetValue(key, out value))
            {
                value = GetValue(value);
                return true;
            }
            return false;
        }

        public object Get(string key, object defaultValue = null)
        {
            object value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }

        public object GetRaw(string key) => _metadata[key];

        public T Get<T>(string key) => (T)Get(key);

        public T Get<T>(string key, T defaultValue) => (T)Get(key, (object)defaultValue);

        public string String(string key, string defaultValue = null) => Get<string>(key, defaultValue);

        public bool Bool(string key, bool defaultValue = false) => Get<bool>(key, defaultValue);

        public DateTime DateTime(string key, DateTime defaultValue = default(DateTime)) => Get<DateTime>(key, defaultValue);

        public FilePath FilePath(string key, FilePath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new FilePath(stringValue) : (FilePath)value;
        }

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null)
        {
            object value = Get(key, (object)defaultValue);
            string stringValue = value as string;
            return stringValue != null ? new DirectoryPath(stringValue) : (DirectoryPath)value;
        }

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
                    throw new KeyNotFoundException();
                }
                return value;
            }
        }

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Select(x => GetValue(x.Value));

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.Select(GetItem).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _metadata.Count;

        /// <summary>
        /// This resolves the metadata value by recursivly expanding IMetadataValue.
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

        public void Add(KeyValuePair<string, object> item) => _metadata.Add(item);

        public void Clear() => _metadata.Clear();

        public bool Contains(KeyValuePair<string, object> item) => _metadata.Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _metadata.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, object> item) => _metadata.Remove(item);

        public bool IsReadOnly => false;

        public void Add(string key, object value) => _metadata.Add(key, value);

        public bool Remove(string key) => _metadata.Remove(key);

        object IDictionary<string, object>.this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys => _metadata.Keys;

        ICollection<object> IDictionary<string, object>.Values => _metadata.Values;

        public IMetadata<T> MetadataAs<T>()
        {
            throw new NotImplementedException();
        }

        ICollection<string> IMetadataDictionary.Keys
        {
            get { throw new NotImplementedException(); }
        }

        ICollection<object> IMetadataDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        object IMetadataDictionary.this[string key]
        {
            get { return _metadata[key]; }
            set { _metadata[key] = value; }
        }

        [Obsolete]
        string IReadOnlySettings.Host
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool IReadOnlySettings.LinksUseHttps
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        DirectoryPath IReadOnlySettings.LinkRoot
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool IReadOnlySettings.LinkHideIndexPages
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool IReadOnlySettings.LinkHideExtensions
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool IReadOnlySettings.UseCache
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool IReadOnlySettings.CleanOutputPath
        {
            get { throw new NotImplementedException(); }
        }

        [Obsolete]
        string ISettings.Host
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool ISettings.LinksUseHttps
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        DirectoryPath ISettings.LinkRoot
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool ISettings.LinkHideIndexPages
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool ISettings.LinkHideExtensions
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool ISettings.UseCache
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Obsolete]
        bool ISettings.CleanOutputPath
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
