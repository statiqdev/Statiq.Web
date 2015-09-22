using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Wyam.Modules.CodeAnalysis
{
    // This class is mutable until we need it to be immutable, then we can just flip it over
    internal class MutableReadOnlyList<T> : IReadOnlyList<T>
    {
        private ConcurrentBag<T> _bag;
        private IImmutableList<T> _immutableList;

        public MutableReadOnlyList()
        {
            _bag = new ConcurrentBag<T>();
        }

        public void Add(T item)
        {
            if (_bag == null)
            {
                throw new InvalidOperationException("The list has already been made immutable.");
            }
            _bag.Add(item);
        }

        public void MakeImmutable()
        {
            if (_bag == null)
            {
                throw new InvalidOperationException("The list has already been made immutable.");
            }
            _immutableList = _bag.ToImmutableList();
            _bag = null;
        }

        private void CheckImmutable()
        {
            if (_immutableList == null)
            {
                throw new InvalidOperationException("The list must be made immutable before calling this method.");
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            CheckImmutable();
            return _immutableList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                CheckImmutable();
                return _immutableList.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                CheckImmutable();
                return _immutableList[index];
            }
        }
    }
}