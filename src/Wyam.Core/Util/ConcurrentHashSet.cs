using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Util
{
    public class ConcurrentHashSet<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        private readonly ConcurrentDictionary<T, byte> _dictionary;

        public ConcurrentHashSet()
        {
            _dictionary = new ConcurrentDictionary<T, byte>();
        }

        public ConcurrentHashSet(IEnumerable<T> collection)
        {
            _dictionary = new ConcurrentDictionary<T, byte>(
                collection.Select(x => new KeyValuePair<T, byte>(x, Byte.MinValue)));
        }

        public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _dictionary = new ConcurrentDictionary<T, byte>(
                collection.Select(x => new KeyValuePair<T, byte>(x, Byte.MinValue)),
                comparer);
        }

        public ConcurrentHashSet(IEqualityComparer<T> comparer)
        {
            _dictionary = new ConcurrentDictionary<T, byte>(comparer);
        }

        public bool Add(T item) => _dictionary.TryAdd(item, Byte.MinValue);

        // IEnumerable, IEnumerable<T>

        public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // IReadOnlyCollection<T>

        public int Count => _dictionary.Count;

        // ICollection<T>

        void ICollection<T>.Add(T item) => ((IDictionary<T, byte>) _dictionary).Add(item, Byte.MinValue);

        public void Clear() => _dictionary.Clear();

        public bool Contains(T item) => _dictionary.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex) => _dictionary.Keys.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            byte value;
            return _dictionary.TryRemove(item, out value);
        }

        public bool IsReadOnly => false;
    }
}
