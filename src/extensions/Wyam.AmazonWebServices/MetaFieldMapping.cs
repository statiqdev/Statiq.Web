using System;

namespace Wyam.AmazonWebServices
{
    internal class MetaFieldMapping
    {
        public string FieldName { get; }
        public string MetaKey { get; }

        public Func<object, object> Transformer { get; }

        public MetaFieldMapping(string fieldName, string metaKey, Func<object, object> transformer = null)
        {
            FieldName = fieldName;
            MetaKey = metaKey;

            // If they didn't pass in a transformer, just set it to a function which returns the input unchanged
            Transformer = transformer ?? (o => o);
        }
    }
}