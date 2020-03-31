using Statiq.Common;

namespace Statiq.Aws
{
    internal class Field
    {
        public Config<string> FieldName { get; }
        public Config<object> FieldValue { get; }

        public Field(Config<string> fieldName, Config<object> fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }
    }
}