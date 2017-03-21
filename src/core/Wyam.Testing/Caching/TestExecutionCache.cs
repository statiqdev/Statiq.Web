using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Caching;
using Wyam.Common.Documents;

namespace Wyam.Testing.Caching
{
    /// <summary>
    /// A test cache that always misses.
    /// </summary>
    public class TestExecutionCache : IExecutionCache
    {
        /// <inheritdoc />
        public bool ContainsKey(IDocument document)
        {
            return false;
        }

        /// <inheritdoc />
        public bool ContainsKey(IDocument document, string key)
        {
            return false;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(IDocument document, out object value)
        {
            value = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(IDocument document, string key, out object value)
        {
            value = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue<TValue>(IDocument document, string key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        /// <inheritdoc />
        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        /// <inheritdoc />
        public void Set(IDocument document, object value)
        {
        }

        /// <inheritdoc />
        public void Set(IDocument document, string key, object value)
        {
        }

        /// <inheritdoc />
        public void Set(string key, object value)
        {
        }
    }
}
