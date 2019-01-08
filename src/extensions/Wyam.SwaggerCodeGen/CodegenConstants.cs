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

namespace Wyam.SwaggerCodeGen
{
    public static class CodegenConstants
    {
        /* System Properties */
        // NOTE: We may want to move these to a separate class to avoid confusion or modification.
        public static string APIS = "apis";
        public static string MODELS = "models";
        public static string SUPPORTING_FILES = "supportingFiles";
        public static string MODEL_TESTS = "modelTests";
        public static string MODEL_DOCS = "modelDocs";
        public static string API_TESTS = "apiTests";
        public static string API_DOCS = "apiDocs";
        public static string WITH_XML = "withXml";
        /* /end System Properties */

        public static string API_PACKAGE = "apiPackage";
        public static string API_PACKAGE_DESC = "package for generated api classes";

        public static string MODEL_PACKAGE = "modelPackage";
        public static string MODEL_PACKAGE_DESC = "package for generated models";

        public static string TEMPLATE_DIR = "templateDir";

        public static string ALLOW_UNICODE_IDENTIFIERS = "allowUnicodeIdentifiers";
        public static string ALLOW_UNICODE_IDENTIFIERS_DESC = "boolean, toggles whether unicode identifiers are allowed in names or not, default is false";

        public static string INVOKER_PACKAGE = "invokerPackage";
        public static string INVOKER_PACKAGE_DESC = "root package for generated code";

        public static string PHP_INVOKER_PACKAGE = "phpInvokerPackage";
        public static string PHP_INVOKER_PACKAGE_DESC = "root package for generated php code";

        public static string PERL_MODULE_NAME = "perlModuleName";
        public static string PERL_MODULE_NAME_DESC = "root module name for generated perl code";

        public static string PYTHON_PACKAGE_NAME = "pythonPackageName";
        public static string PYTHON_PACKAGE_NAME_DESC = "package name for generated python code";

        public static string GROUP_ID = "groupId";
        public static string GROUP_ID_DESC = "groupId in generated pom.xml";

        public static string ARTIFACT_ID = "artifactId";
        public static string ARTIFACT_ID_DESC = "artifactId in generated pom.xml";

        public static string ARTIFACT_VERSION = "artifactVersion";
        public static string ARTIFACT_VERSION_DESC = "artifact version in generated pom.xml";

        public static string ARTIFACT_URL = "artifactUrl";
        public static string ARTIFACT_URL_DESC = "artifact URL in generated pom.xml";

        public static string ARTIFACT_DESCRIPTION = "artifactDescription";
        public static string ARTIFACT_DESCRIPTION_DESC = "artifact description in generated pom.xml";

        public static string SCM_CONNECTION = "scmConnection";
        public static string SCM_CONNECTION_DESC = "SCM connection in generated pom.xml";

        public static string SCM_DEVELOPER_CONNECTION = "scmDeveloperConnection";
        public static string SCM_DEVELOPER_CONNECTION_DESC = "SCM developer connection in generated pom.xml";

        public static string SCM_URL = "scmUrl";
        public static string SCM_URL_DESC = "SCM URL in generated pom.xml";

        public static string DEVELOPER_NAME = "developerName";
        public static string DEVELOPER_NAME_DESC = "developer name in generated pom.xml";

        public static string DEVELOPER_EMAIL = "developerEmail";
        public static string DEVELOPER_EMAIL_DESC = "developer email in generated pom.xml";

        public static string DEVELOPER_ORGANIZATION = "developerOrganization";
        public static string DEVELOPER_ORGANIZATION_DESC = "developer organization in generated pom.xml";

        public static string DEVELOPER_ORGANIZATION_URL = "developerOrganizationUrl";
        public static string DEVELOPER_ORGANIZATION_URL_DESC = "developer organization URL in generated pom.xml";

        public static string LICENSE_NAME = "licenseName";
        public static string LICENSE_NAME_DESC = "The name of the license";

        public static string LICENSE_URL = "licenseUrl";
        public static string LICENSE_URL_DESC = "The URL of the license";

        public static string SOURCE_FOLDER = "sourceFolder";
        public static string SOURCE_FOLDER_DESC = "source folder for generated code";

        public static string IMPL_FOLDER = "implFolder";
        public static string IMPL_FOLDER_DESC = "folder for generated implementation code";

        public static string LOCAL_VARIABLE_PREFIX = "localVariablePrefix";
        public static string LOCAL_VARIABLE_PREFIX_DESC = "prefix for generated code members and local variables";

        public static string SERIALIZABLE_MODEL = "serializableModel";
        public static string SERIALIZABLE_MODEL_DESC = "boolean - toggle \"implements Serializable\" for generated models";

        public static string SERIALIZE_BIG_DECIMAL_AS_STRING = "bigDecimalAsString";
        public static string SERIALIZE_BIG_DECIMAL_AS_STRING_DESC = "Treat BigDecimal values as Strings to avoid precision loss.";

        public static string LIBRARY = "library";
        public static string LIBRARY_DESC = "library template (sub-template)";

        public static string SORT_PARAMS_BY_REQUIRED_FLAG = "sortParamsByRequiredFlag";
        public static string SORT_PARAMS_BY_REQUIRED_FLAG_DESC = "Sort method arguments to place required parameters before optional parameters.";

        public static string USE_DATETIME_OFFSET = "useDateTimeOffset";
        public static string USE_DATETIME_OFFSET_DESC = "Use DateTimeOffset to model date-time properties";

        public static string ENSURE_UNIQUE_PARAMS = "ensureUniqueParams";
        public static string ENSURE_UNIQUE_PARAMS_DESC = "Whether to ensure parameter names are unique in an operation (rename parameters that are not).";

        public static string PROJECT_NAME = "projectName";
        public static string PACKAGE_NAME = "packageName";
        public static string PACKAGE_VERSION = "packageVersion";

        public static string PACKAGE_TITLE = "packageTitle";
        public static string PACKAGE_TITLE_DESC = "Specifies an AssemblyTitle for the .NET Framework global assembly attributes stored in the AssemblyInfo file.";
        public static string PACKAGE_PRODUCTNAME = "packageProductName";
        public static string PACKAGE_PRODUCTNAME_DESC = "Specifies an AssemblyProduct for the .NET Framework global assembly attributes stored in the AssemblyInfo file.";
        public static string PACKAGE_DESCRIPTION = "packageDescription";
        public static string PACKAGE_DESCRIPTION_DESC = "Specifies a AssemblyDescription for the .NET Framework global assembly attributes stored in the AssemblyInfo file.";
        public static string PACKAGE_COMPANY = "packageCompany";
        public static string PACKAGE_COMPANY_DESC = "Specifies an AssemblyCompany for the .NET Framework global assembly attributes stored in the AssemblyInfo file.";
        public static string PACKAGE_AUTHORS = "packageAuthors";
        public static string PACKAGE_AUTHORS_DESC = "Specifies Authors property in the .NET Core project file.";
        public static string PACKAGE_COPYRIGHT = "packageCopyright";
        public static string PACKAGE_COPYRIGHT_DESC = "Specifies an AssemblyCopyright for the .NET Framework global assembly attributes stored in the AssemblyInfo file.";

        public static string POD_VERSION = "podVersion";

        public static string OPTIONAL_METHOD_ARGUMENT = "optionalMethodArgument";
        public static string OPTIONAL_METHOD_ARGUMENT_DESC = "Optional method argument, e.g. void square(int x=10) (.net 4.0+ only).";

        public static string OPTIONAL_ASSEMBLY_INFO = "optionalAssemblyInfo";
        public static string OPTIONAL_ASSEMBLY_INFO_DESC = "Generate AssemblyInfo.cs.";

        public static string NETCORE_PROJECT_FILE = "netCoreProjectFile";
        public static string NETCORE_PROJECT_FILE_DESC = "Use the new format (.NET Core) for .NET project files (.csproj).";

        public static string USE_COLLECTION = "useCollection";
        public static string USE_COLLECTION_DESC = "Deserialize array types to Collection<T> instead of List<T>.";

        public static string INTERFACE_PREFIX = "interfacePrefix";
        public static string INTERFACE_PREFIX_DESC = "Prefix interfaces with a community standard or widely accepted prefix.";

        public static string RETURN_ICOLLECTION = "returnICollection";
        public static string RETURN_ICOLLECTION_DESC = "Return ICollection<T> instead of the concrete type.";

        public static string OPTIONAL_PROJECT_FILE = "optionalProjectFile";
        public static string OPTIONAL_PROJECT_FILE_DESC = "Generate {PackageName}.csproj.";

        public static string OPTIONAL_PROJECT_GUID = "packageGuid";
        public static string OPTIONAL_PROJECT_GUID_DESC = "The GUID that will be associated with the C# project";

        public static string MODEL_PROPERTY_NAMING = "modelPropertyNaming";
        public static string MODEL_PROPERTY_NAMING_DESC = "Naming convention for the property: 'camelCase', 'PascalCase', 'snake_case' and 'original', which keeps the original name";

        public static string DOTNET_FRAMEWORK = "targetFramework";
        public static string DOTNET_FRAMEWORK_DESC = "The target .NET framework version.";

        public enum MODEL_PROPERTY_NAMING_TYPE { camelCase, PascalCase, snake_case, original }
        public enum ENUM_PROPERTY_NAMING_TYPE { camelCase, PascalCase, snake_case, original, UPPERCASE }

        public static string ENUM_PROPERTY_NAMING = "enumPropertyNaming";
        public static string ENUM_PROPERTY_NAMING_DESC = "Naming convention for enum properties: 'camelCase', 'PascalCase', 'snake_case', 'UPPERCASE', and 'original'";

        public static string MODEL_NAME_PREFIX = "modelNamePrefix";
        public static string MODEL_NAME_PREFIX_DESC = "Prefix that will be prepended to all model names. Default is the empty string.";

        public static string MODEL_NAME_SUFFIX = "modelNameSuffix";
        public static string MODEL_NAME_SUFFIX_DESC = "Suffix that will be appended to all model names. Default is the empty string.";

        public static string OPTIONAL_EMIT_DEFAULT_VALUES = "optionalEmitDefaultValues";
        public static string OPTIONAL_EMIT_DEFAULT_VALUES_DESC = "Set DataMember's EmitDefaultValue.";

        public static string GIT_USER_ID = "gitUserId";
        public static string GIT_USER_ID_DESC = "Git user ID, e.g. swagger-api.";

        public static string GIT_REPO_ID = "gitRepoId";
        public static string GIT_REPO_ID_DESC = "Git repo ID, e.g. swagger-codegen.";

        public static string RELEASE_NOTE = "releaseNote";
        public static string RELEASE_NOTE_DESC = "Release note, default to 'Minor update'.";

        public static string HTTP_USER_AGENT = "httpUserAgent";
        public static string HTTP_USER_AGENT_DESC = "HTTP user agent, e.g. codegen_csharp_api_client, default to 'Swagger-Codegen/{packageVersion}}/{language}'";

        public static string SUPPORTS_ES6 = "supportsES6";
        public static string SUPPORTS_ES6_DESC = "Generate code that conforms to ES6.";

        public static string SUPPORTS_ASYNC = "supportsAsync";
        public static string SUPPORTS_ASYNC_DESC = "Generate code that supports async operations.";

        public static string EXCLUDE_TESTS = "excludeTests";
        public static string EXCLUDE_TESTS_DESC = "Specifies that no tests are to be generated.";

        // Not user-configurable. System provided for use in templates.
        public static string GENERATE_API_DOCS = "generateApiDocs";

        public static string GENERATE_API_TESTS = "generateApiTests";
        public static string GENERATE_API_TESTS_DESC = "Specifies that api tests are to be generated.";

        // Not user-configurable. System provided for use in templates.
        public static string GENERATE_MODEL_DOCS = "generateModelDocs";

        public static string GENERATE_MODEL_TESTS = "generateModelTests";
        public static string GENERATE_MODEL_TESTS_DESC = "Specifies that model tests are to be generated.";

        public static string HIDE_GENERATION_TIMESTAMP = "hideGenerationTimestamp";
        public static string HIDE_GENERATION_TIMESTAMP_DESC = "Hides the generation timestamp when files are generated.";

        public static string GENERATE_PROPERTY_CHANGED = "generatePropertyChanged";
        public static string GENERATE_PROPERTY_CHANGED_DESC = "Specifies that models support raising property changed events.";

        public static string NON_PUBLIC_API = "nonPublicApi";
        public static string NON_PUBLIC_API_DESC = "Generates code with reduced access modifiers; allows embedding elsewhere without exposing non-public API calls to consumers.";

        public static string VALIDATABLE = "validatable";
        public static string VALIDATABLE_DESC = "Generates self-validatable models.";

        public static string IGNORE_FILE_OVERRIDE = "ignoreFileOverride";
        public static string IGNORE_FILE_OVERRIDE_DESC = "Specifies an override location for the .swagger-codegen-ignore file. Most useful on initial generation.";

        public static string REMOVE_OPERATION_ID_PREFIX = "removeOperationIdPrefix";
        public static string REMOVE_OPERATION_ID_PREFIX_DESC = "Remove prefix of operationId, e.g. config_getId => getId";
    }
}
