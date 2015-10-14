using System;

namespace Wyam.Common.Documents
{
    // This uses a delegate to get the metadata value
    // Note that the provided delegate should be thread-safe
    public class DelegateMetadataValue : IMetadataValue
    {
        private readonly Func<string, IMetadata, object> _value;

        public DelegateMetadataValue(Func<string, IMetadata, object> value)
        {
            _value = value;
        }

        public virtual object Get(string key, IMetadata metadata)
        {
            return _value(key, metadata);
        }
    }
}
