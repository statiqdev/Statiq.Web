using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    public interface IMetadata : IReadOnlyDictionary<string, object>
    {
        // This is a safe way to get metadata values - it returns the specified default value if the key is not found
        object Get(string key, object defaultValue = null);
    }
}
