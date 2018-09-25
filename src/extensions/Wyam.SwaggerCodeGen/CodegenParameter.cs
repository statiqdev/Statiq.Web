using System.Collections.Generic;

namespace Wyam.SwaggerCodeGen
{
    public class CodegenParameter
    {
        public bool isFormParam;
        public bool isQueryParam;
        public bool isPathParam;
        public bool isHeaderParam;
        public bool isCookieParam;
        public bool isBodyParam;
        public bool hasMore;
        public bool isContainer;
        public bool secondaryParam;
        public bool isCollectionFormatMulti;
        public bool isPrimitiveType;
        public string baseName;
        public string paramName;
        public string dataType;
        public string datatypeWithEnum;
        public string dataFormat;
        public string collectionFormat;
        public string description;
        public string unescapedDescription;
        public string baseType;
        public string defaultValue;
        public string enumName;

        public string example; // example value (x-example)
        public string jsonSchema;
        public bool isstring, isNumeric, isint, isLong, isNumber, isFloat, isDouble, isByteArray, isBinary, isbool, isDate, isDateTime, isUuid;
        public bool isListContainer, isMapContainer;
        public bool isFile, notFile;
        public bool isEnum;
        public List<string> _enum;
        public Dictionary<string, object> allowableValues;
        public CodegenProperty items;
        public Dictionary<string, object> vendorExtensions;
        public bool hasValidation;

        /**
         * Determines whether this parameter is mandatory. If the parameter is in "path",
         * this property is required and its value MUST be true. Otherwise, the property
         * MAY be included and its default value is false.
         */
        public bool required;

        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor17.
         */
        public string maximum;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor17
         */
        public bool exclusiveMaximum;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor21
         */
        public string minimum;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor21
         */
        public bool exclusiveMinimum;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor26
         */
        public int maxLength;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor29
         */
        public int minLength;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor33
         */
        public string pattern;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor42
         */
        public int maxItems;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor45
         */
        public int minItems;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor49
         */
        public bool uniqueItems;
        /**
         * See http://json-schema.org/latest/json-schema-validation.html#anchor14
         */
        public decimal multipleOf;

        public CodegenParameter copy()
        {
            CodegenParameter output = new CodegenParameter();
            output.isFile = isFile;
            output.notFile = notFile;
            output.hasMore = hasMore;
            output.isContainer = isContainer;
            output.secondaryParam = secondaryParam;
            output.baseName = baseName;
            output.paramName = paramName;
            output.dataType = dataType;
            output.datatypeWithEnum = datatypeWithEnum;
            output.enumName = enumName;
            output.dataFormat = dataFormat;
            output.collectionFormat = collectionFormat;
            output.isCollectionFormatMulti = isCollectionFormatMulti;
            output.isPrimitiveType = isPrimitiveType;
            output.description = description;
            output.unescapedDescription = unescapedDescription;
            output.baseType = baseType;
            output.isFormParam = isFormParam;
            output.isQueryParam = isQueryParam;
            output.isPathParam = isPathParam;
            output.isHeaderParam = isHeaderParam;
            output.isCookieParam = isCookieParam;
            output.isBodyParam = isBodyParam;
            output.required = required;
            output.maximum = maximum;
            output.exclusiveMaximum = exclusiveMaximum;
            output.minimum = minimum;
            output.exclusiveMinimum = exclusiveMinimum;
            output.maxLength = maxLength;
            output.minLength = minLength;
            output.pattern = pattern;
            output.maxItems = maxItems;
            output.minItems = minItems;
            output.uniqueItems = uniqueItems;
            output.multipleOf = multipleOf;
            output.jsonSchema = jsonSchema;
            output.defaultValue = defaultValue;
            output.example = example;
            output.isEnum = isEnum;
            if (_enum != null)
            {
                output._enum = new List<string>(_enum);
            }
            if (allowableValues != null)
            {
                output.allowableValues = new Dictionary<string, object>(allowableValues);
            }
            if (items != null)
            {
                output.items = items;
            }
            if (vendorExtensions != null)
            {
                output.vendorExtensions = new Dictionary<string, object>(vendorExtensions);
            }

            output.hasValidation = hasValidation;
            output.isBinary = isBinary;
            output.isByteArray = isByteArray;
            output.isstring = isstring;
            output.isNumeric = isNumeric;
            output.isint = isint;
            output.isLong = isLong;
            output.isDouble = isDouble;
            output.isFloat = isFloat;
            output.isNumber = isNumber;
            output.isbool = isbool;
            output.isDate = isDate;
            output.isDateTime = isDateTime;
            output.isUuid = isUuid;
            output.isListContainer = isListContainer;
            output.isMapContainer = isMapContainer;

            return output;
        }

        public override string ToString() => $"{baseName}({dataType})";
    }
}