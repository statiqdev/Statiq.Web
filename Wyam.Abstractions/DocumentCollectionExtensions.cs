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
    }
}
