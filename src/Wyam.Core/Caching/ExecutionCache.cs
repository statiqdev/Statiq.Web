using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Tracing;

namespace Wyam.Core.Caching
{
    internal class ExecutionCache : Cache<object>, IExecutionCache
    {
        public bool TryGetValue<TValue>(IDocument document, out TValue value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return TryGetValue<TValue>(GetDocumentKey(document), out value);
        }

        public bool TryGetValue<TValue>(IDocument document, string key, out TValue value)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return TryGetValue<TValue>(GetDocumentKey(document, key), out value);
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            object rawValue;
            bool result = base.TryGetValue(key, out rawValue);
            value = (TValue) rawValue;
            Trace.Verbose("Cache {0} for key: {1}", result ? "hit" : "miss", key);
            return result;
        }

        public override bool TryGetValue(string key, out object value)
        {
            bool result = base.TryGetValue(key, out value);
            Trace.Verbose("Cache {0} for key: {1}", result ? "hit" : "miss", key);
            return result;
        }

        public override void Set(string key, object value)
        {
            base.Set(key, value);
            Trace.Verbose("Cache set for key: {0}", key);
        }
    }
}
