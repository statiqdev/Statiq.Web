using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IExecutionCache
    {
        bool TryGetValue(IDocument document, out object value);
        bool TryGetValue<TValue>(IDocument document, out TValue value);
        void Set(IDocument document, object value);
        bool TryGetValue(string key, out object value);
        bool TryGetValue<TValue>(string key, out TValue value);
        void Set(string key, object value);
    }
}
