using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// A collection of metadata items.
    /// </summary>
    public class MetadataItems : IList<MetadataItem>, IList<KeyValuePair<string, object>>
    {
        private readonly List<MetadataItem> _items = new List<MetadataItem>();

        // IList<MetadataItem>

        /// <inheritdoc />
        public IEnumerator<MetadataItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            _items.Add(new MetadataItem(key, value));
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> pair)
        {
            _items.Add(new MetadataItem(pair));
        }

        /// <inheritdoc />
        public void Add(string key, Func<IMetadata, object> value, bool cacheValue = false)
        {
            _items.Add(new MetadataItem(key, value, cacheValue));
        }

        /// <inheritdoc />
        public void Add(MetadataItem item)
        {
            _items.Add(item);
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<MetadataItem> items)
        {
            _items.AddRange(items);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _items.Clear();
        }

        /// <inheritdoc />
        public bool Contains(MetadataItem item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(MetadataItem[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(MetadataItem item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(MetadataItem item)
        {
            return _items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, MetadataItem item)
        {
            _items.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        /// <inheritdoc />
        public MetadataItem this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        // IList<KeyValuePair<string, object>>

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _items.Select(x => x.Pair).GetEnumerator();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _items.Select(x => x.Pair).ToList().CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item)
        {
            return _items.Remove(item);
        }

        /// <inheritdoc />
        public int IndexOf(KeyValuePair<string, object> item)
        {
            return _items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, KeyValuePair<string, object> item)
        {
            _items.Insert(index, item);
        }

        /// <inheritdoc />
        KeyValuePair<string, object> IList<KeyValuePair<string, object>>.this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }
    }
}