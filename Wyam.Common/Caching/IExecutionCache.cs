using Wyam.Common.Documents;

namespace Wyam.Common.Caching
{
    public interface IExecutionCache
    {
        bool ContainsKey(IDocument document);
        bool ContainsKey(string key);

        bool TryGetValue(IDocument document, out object value);
        bool TryGetValue(string key, out object value);

        bool TryGetValue<TValue>(IDocument document, out TValue value);
        bool TryGetValue<TValue>(string key, out TValue value);

        void Set(IDocument document, object value);
        void Set(string key, object value);
    }
}
