using System;
using Wyam.Common.Documents;

namespace Wyam.AmazonWebServices
{
    internal class Field
    {
        public string FieldName { get; }
        public object FieldValue { get; }
        public Func<IDocument, object> Execute { get; }

        public Field(string fieldName, object fieldValue)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
        }

        public Field(string fieldName, Func<IDocument, object> execute)
        {
            FieldName = fieldName;
            Execute = execute;
        }

        public object GetValue(IDocument doc)
        {
            // Return the field value, if it's set
            // Note: if the field value is set, the passed-in document is never used...
            if (FieldValue != null)
            {
                return FieldValue;
            }

            return Execute.Invoke(doc);
        }
    }
}