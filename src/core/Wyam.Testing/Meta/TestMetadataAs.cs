using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Testing.Meta
{
    internal class TestMetadataAs<T> : Dictionary<string, T>, IMetadata<T>
    {
        public TestMetadataAs()
        {
        }

        public TestMetadataAs(IDictionary<string, T> dictionary)
            : base(dictionary)
        {
        }

        public T Get(string key)
        {
            T value;
            return TryGetValue(key, out value) ? value : default(T);
        }

        public T Get(string key, T defaultValue)
        {
            T value;
            return TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
