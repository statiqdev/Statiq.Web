using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.SwaggerCodeGen
{
    public class CodegenProperty
    {
        public string baseName;
        public string complexType;
        public string getter;
        public string setter;
        public string description;
        public string datatype;
        public string datatypeWithEnum;
        public string dataFormat;
        public string name;
        public string min;
        public string max;
        public string defaultValue;
        public string defaultValueWithParam;
        public string baseType;
        public string containerType;
        public string title;

        /** The 'description' string without escape charcters needed by some programming languages/targets */
        public string unescapedDescription;

        /**
         * maxLength validation for strings, see http://json-schema.org/latest/json-schema-validation.html#rfc.section.5.2.1
         */
        public int maxLength;
        /**
         * minLength validation for strings, see http://json-schema.org/latest/json-schema-validation.html#rfc.section.5.2.2
         */
        public int minLength;
        /**
         * pattern validation for strings, see http://json-schema.org/latest/json-schema-validation.html#rfc.section.5.2.3
         */
        public string pattern;
        /**
         * A free-form property to include an example of an instance for this schema.
         */
        public string example;

        public string jsonSchema;
        public string minimum;
        public string maximum;
        public bool exclusiveMinimum;
        public bool exclusiveMaximum;
        public bool hasMore;
        public bool required;
        public bool secondaryParam;
        public bool hasMoreNonReadOnly; // for model constructor, true if next properyt is not readonly
        public bool isPrimitiveType;
        public bool isContainer;
        public bool isNotContainer;
        public bool isstring;
        public bool isNumeric;
        public bool isint;
        public bool isLong;
        public bool isNumber;
        public bool isFloat;
        public bool isDouble;
        public bool isByteArray;
        public bool isBinary;
        public bool isFile;
        public bool isbool;
        public bool isDate;
        public bool isDateTime;
        public bool isUuid;
        public bool isListContainer;
        public bool isMapContainer;
        public bool isEnum;
        public bool isReadOnly = false;
        public List<string> _enum;
        public Dictionary<string, object> allowableValues;
        public CodegenProperty items;
        public Dictionary<string, object> vendorExtensions;
        public bool hasValidation; // true if pattern, maximum, etc are set (only used in the mustache template)
        public bool isInherited;
        public string discriminatorValue;
        public string nameInCamelCase; // property name in camel case
                                       // enum name based on the property name, usually use as a prefix (e.g. VAR_NAME) for enum name (e.g. VAR_NAME_VALUE1)
        public string enumName;
        public int maxItems;
        public int minItems;

        // XML
        public bool isXmlAttribute = false;
        public string xmlPrefix;
        public string xmlName;
        public string xmlNamespace;
        public bool isXmlWrapped = false;


        public override string ToString() => $"{baseName}({datatype})";

    }
}
