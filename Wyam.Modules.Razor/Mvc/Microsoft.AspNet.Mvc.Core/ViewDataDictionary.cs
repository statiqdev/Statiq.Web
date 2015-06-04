// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc
{
    public class ViewDataDictionary : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _data;

        public ViewDataDictionary()
        {
            _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        #region IDictionary properties
        // Do not just pass through to _data: Indexer should not throw a KeyNotFoundException.
        public object this[string index]
        {
            get
            {
                object result;
                _data.TryGetValue(index, out result);
                return result;
            }
            set
            {
                _data[index] = value;
            }
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return _data.IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return _data.Keys; }
        }

        public ICollection<object> Values
        {
            get { return _data.Values; }
        }
        #endregion

        #region IDictionary methods
        public void Add([NotNull] string key, object value)
        {
            _data.Add(key, value);
        }

        public bool ContainsKey([NotNull] string key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove([NotNull] string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue([NotNull] string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        public void Add([NotNull] KeyValuePair<string, object> item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains([NotNull] KeyValuePair<string, object> item)
        {
            return _data.Contains(item);
        }

        public void CopyTo([NotNull] KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove([NotNull] KeyValuePair<string, object> item)
        {
            return _data.Remove(item);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
        #endregion
    }
}