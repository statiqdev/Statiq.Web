using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
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

        public static ILookup<T, IDocument> ToLookup<T>(this IEnumerable<IDocument> documents, string key)
        {
            // Get a mapping of all documents with the key to their values
            Dictionary<IDocument, IEnumerable<T>> documentValues = documents
                .Distinct()
                .Select(x =>
                {
                    IEnumerable<T> values;
                    return new KeyValuePair<IDocument, IEnumerable<T>>(x, x.MetadataAs<IEnumerable<T>>().TryGetValue(key, out values) ? values : null);
                })
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Key, x => x.Value);

            // Now invert the dictionary to a lookup of values to documents
            return documentValues
                .SelectMany(x => x.Value)
                .Distinct()
                .SelectMany(x => documentValues.Where(y => y.Value.Contains(x)).Select(y => new KeyValuePair<T, IDocument>(x, y.Key)))
                .ToLookup(x => x.Key, x => x.Value);
        }
    }
}
