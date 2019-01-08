using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

[assembly: SuppressMessage("", "RCS1008", Justification = "Stop !")]
[assembly: SuppressMessage("", "RCS1009", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1503", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1401", Justification = "Stop !")]
[assembly: SuppressMessage("", "IDE0008", Justification = "Stop !")]
[assembly: SuppressMessage("", "RCS1012", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1401", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1310", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1300", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1136", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1502", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1307", Justification = "Stop !")]

namespace Wyam.SwaggerCodeGen
{
    public class CodegenModel
    {
        public string parent;
        public string parentSchema;
        public List<string> interfaces;

        // References to parent and interface CodegenModels. Only set when code generator supports inheritance.
        public CodegenModel parentModel;
        public List<CodegenModel> interfaceModels = new List<CodegenModel>();
        public List<CodegenModel> children = new List<CodegenModel>();

        public string name;
        public string classname;
        public string title;
        public string description;
        public string classVarName;
        public string modelJson;
        public string dataType;
        public string xmlPrefix;
        public string xmlNamespace;
        public string xmlName;
        public string classFilename; // store the class file name, mainly used for import
        public string unescapedDescription;
        public string discriminator;
        public string defaultValue;
        public string arrayModelType;
        public bool isAlias; // Is this effectively an alias of another simple type
        public List<CodegenProperty> vars = new List<CodegenProperty>();
        public List<CodegenProperty> allVars;

        public List<CodegenProperty> requiredVars = new List<CodegenProperty>(); // a list of required properties
        public List<CodegenProperty> optionalVars = new List<CodegenProperty>(); // a list of optional properties
        public List<CodegenProperty> readOnlyVars = new List<CodegenProperty>(); // a list of read-only properties
        public List<CodegenProperty> readWriteVars = new List<CodegenProperty>(); // a list of properties for read, write
        public List<CodegenProperty> parentVars = new List<CodegenProperty>();
        public Dictionary<string, object> allowableValues;

        // Sorted sets of required parameters.
        public List<string> mandatory = new List<string>();
        public List<string> allMandatory;

        public List<string> imports = new List<string>();
        public bool hasVars;
        public bool emptyVars;
        public bool hasMoreModels;
        public bool hasEnums;
        public bool isEnum;
        public bool hasRequired;
        public bool hasOptional;
        public bool isArrayModel;
        public bool hasChildren;
        public bool hasOnlyReadOnly = true; // true if all properties are read-only
        public ExternalDocs externalDocs;

        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();

        //The type of the value from additional properties. Used in map like objects.
        public string additionalPropertiesType;

        public CodegenModel()
        {
            // By default these are the same collections. Where the code generator supports inheritance, composed models
            // store the complete closure of owned and inherited properties in allVars and allMandatory.
            allVars = vars;
            allMandatory = mandatory;
        }

        public override string ToString() => $"{name}({classname})";
    }
}
