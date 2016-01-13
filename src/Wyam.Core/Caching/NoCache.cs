using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;

namespace Wyam.Core.Caching
{
    internal class NoCache : IExecutionCache
    {
        public bool ContainsKey(IDocument document)
        {
            return false;
        }

        public bool ContainsKey(IDocument document, string key)
        {
            return false;
        }

        public bool ContainsKey(string key)
        {
            return false;
        }

        public bool TryGetValue(IDocument document, out object value)
        {
            value = null;
            return false;
        }

        public bool TryGetValue(IDocument document, string key, out object value)
        {
            value = null;
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public bool TryGetValue<TValue>(IDocument document, string key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public void Set(IDocument document, object value)
        {
        }

        public void Set(IDocument document, string key, object value)
        {
        }

        public void Set(string key, object value)
        {
        }
    }
}
