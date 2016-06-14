using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Meta
{
    /// <summary>
    /// A mutable <see cref="IMetadata"/> implementation that works like a dictionary.
    /// </summary>
    public interface IMetadataDictionary : IDictionary<string, object>, IMetadata
    {
        new int Count { get; }
        new bool ContainsKey(string key);
        new ICollection<string> Keys { get; }
        new ICollection<object> Values { get; }
        new bool TryGetValue(string key, out object value);
        new object this[string key] { get; set; }
    }
}