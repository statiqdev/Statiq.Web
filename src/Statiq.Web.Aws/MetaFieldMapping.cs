using System;
using Statiq.Common;

namespace Statiq.Aws
{
    internal class MetaFieldMapping
    {
        public Config<string> FieldName { get; }
        public Config<string> MetaKey { get; }

        public Func<object, object> Transformer { get; }

        public MetaFieldMapping(Config<string> fieldName, Config<string> metaKey, Func<object, object> transformer = null)
        {
            FieldName = fieldName;
            MetaKey = metaKey;
            Transformer = transformer;
        }
    }
}