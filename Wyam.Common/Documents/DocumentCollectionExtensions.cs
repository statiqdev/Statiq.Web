using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Documents
{
    public static class DocumentCollectionExtensions
    {
        public static IEnumerable<IDocument> ContainsKey(this IEnumerable<IDocument> documents, string key)
        {
            return documents.Where(x => x.ContainsKey(key));
        }

        public static IEnumerable<IDocument> ContainsAllKeys(this IEnumerable<IDocument> documents, params string[] keys)
        {
            return documents.Where(x => keys.All(x.ContainsKey));
        }

        public static IEnumerable<IDocument> ContainsAnyKeys(this IEnumerable<IDocument> documents, params string[] keys)
        {
            return documents.Where(x => keys.Any(x.ContainsKey));
        }

        public static ILookup<TKey, IDocument> ToLookup<TKey>(this IEnumerable<IDocument> documents, string keyMetadataKey)
        {
            // Get a mapping of all documents with the key to their values
            IDictionary<IDocument, IEnumerable<TKey>> documentValues = documents.ToDocumentDictionary<TKey>(keyMetadataKey);

            // Now invert the dictionary to a lookup of values to documents
            return documentValues
                .SelectMany(x => x.Value)
                .Distinct()
                .SelectMany(x => documentValues.Where(
                    y => y.Value.Contains(x)).Select(y => new KeyValuePair<TKey, IDocument>(x, y.Key)))
                .ToLookup(x => x.Key, x => x.Value);
        }

        public static ILookup<TKey, TElement> ToLookup<TKey, TElement>(this IEnumerable<IDocument> documents, string keyMetadataKey, string elementMetadataKey)
        {
            // Get a mapping of all documents with the key to their values
            IDictionary<IDocument, IEnumerable<TKey>> documentValues = documents.ToDocumentDictionary<TKey>(keyMetadataKey);

            // Now invert the dictionary to a lookup of values to documents
            return documentValues
                .SelectMany(x => x.Value)
                .Distinct()
                .SelectMany(x => documentValues
                    .Where(y => y.Value.Contains(x))
                    .Select(y =>
                    {
                        IEnumerable<TElement> values;
                        return new KeyValuePair<TKey, IEnumerable<TElement>>(
                            x, y.Key.MetadataAs<IEnumerable<TElement>>().TryGetValue(elementMetadataKey, out values) ? values : null);
                    })
                    .Where(y => y.Value != null)
                    .SelectMany(y => y.Value.Select(z => new KeyValuePair<TKey, TElement>(y.Key, z)))
                )
                .Distinct()
                .ToLookup(x => x.Key, x => x.Value);
        }

        // Gets a dictionary mapping documents to an IEnumerable<TValue> of all values for a specified metadata key
        public static IDictionary<IDocument, IEnumerable<TValue>> ToDocumentDictionary<TValue>(this IEnumerable<IDocument> documents, string metadataKey)
        {
            return documents
                .Distinct()
                .Select(x =>
                {
                    IEnumerable<TValue> values;
                    return new KeyValuePair<IDocument, IEnumerable<TValue>>(x, x.MetadataAs<IEnumerable<TValue>>().TryGetValue(metadataKey, out values) ? values : null);
                })
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        // Similar to calling the equivalent ToLookup(), but returns the first (in arbitrary order) document for each key
        public static IDictionary<TKey, IDocument> ToDictionary<TKey>(this IEnumerable<IDocument> documents, string keyMetadataKey)
        {
            return documents.ToLookup<TKey>(keyMetadataKey).ToDictionary(x => x.Key, x => x.First());
        }

        // Similar to calling the equivalent ToLookup(), but returns the first (in arbitrary order) item for each key
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<IDocument> documents, string keyMetadataKey, string valueMetadataKey)
        {
            return documents.ToLookup<TKey, TValue>(keyMetadataKey, valueMetadataKey).ToDictionary(x => x.Key, x => x.First());
        }
    }
}
