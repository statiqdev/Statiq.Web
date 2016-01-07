using Wyam.Common.Documents;

namespace Wyam.Common.Caching
{
    public interface IExecutionCache
    {
        bool ContainsKey(IDocument document);
        bool ContainsKey(IDocument document, string key);
        bool ContainsKey(string key);

        bool TryGetValue(IDocument document, out object value);
        bool TryGetValue(IDocument document, string key, out object value);
        bool TryGetValue(string key, out object value);

        bool TryGetValue<TValue>(IDocument document, out TValue value);
        bool TryGetValue<TValue>(IDocument document, string key, out TValue value);
        bool TryGetValue<TValue>(string key, out TValue value);

        void Set(IDocument document, object value);
        void Set(IDocument document, string key, object value);
        void Set(string key, object value);
    }
}
