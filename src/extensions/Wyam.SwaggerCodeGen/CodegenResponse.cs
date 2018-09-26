using System.Collections.Generic;

namespace Wyam.SwaggerCodeGen
{
    public class CodegenResponse
    {
        public readonly List<CodegenProperty> headers = new List<CodegenProperty>();
        public string code;
        public string message;
        public bool hasMore;
        public List<Dictionary<string, object>> examples;
        public string dataType;
        public string baseType;
        public string containerType;
        public bool hasHeaders;
        public bool isstring;
        public bool isNumeric;
        public bool isInteger;
        public bool isLong;
        public bool isNumber;
        public bool isFloat;
        public bool isDouble;
        public bool isByteArray;
        public bool isbool;
        public bool isDate;
        public bool isDateTime;
        public bool isUuid;
        public bool isDefault;
        public bool simpleType;
        public bool primitiveType;
        public bool isMapContainer;
        public bool isListContainer;
        public bool isBinary = false;
        public bool isFile = false;
        public object schema;
        public string jsonSchema;
        public Dictionary<string, object> vendorExtensions;

        public bool isWildcard() => code == "0" || code == "default";

        public override string ToString() => $"{code}({containerType})";
    }
}