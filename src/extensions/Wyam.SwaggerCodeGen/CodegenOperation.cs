using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wyam.SwaggerCodeGen
{
    public class CodegenOperation
    {
        public List<CodegenProperty> responseHeaders = new List<CodegenProperty>();
        public bool hasAuthMethods;
        public bool hasConsumes;
        public bool hasProduces;
        public bool hasParams;
        public bool hasOptionalParams;
        public bool hasRequiredParams;
        public bool returnTypeIsPrimitive;
        public bool returnSimpleType;
        public bool subresourceOperation;
        public bool isMapContainer;
        public bool isListContainer;
        public bool isMultipart;
        public bool hasMore = true;
        public bool isResponseBinary = false;
        public bool isResponseFile = false;
        public bool hasReference = false;
        public bool isRestfulIndex;
        public bool isRestfulShow;
        public bool isRestfulCreate;
        public bool isRestfulUpdate;
        public bool isRestfulDestroy;
        public bool isRestful;
        public bool isDeprecated;
        public string path;
        public string operationId;
        public string returnType;
        public string httpMethod;
        public string returnBaseType;
        public string returnContainer;
        public string summary;
        public string unescapedNotes;
        public string notes;
        public string baseName;
        public string defaultResponse;
        public string discriminator;
        public List<Dictionary<string, string>> consumes;
        public List<Dictionary<string, string>> produces;
        public List<Dictionary<string, string>> prioritizedContentTypes;
        public CodegenParameter bodyParam;
        public List<CodegenParameter> allParams = new List<CodegenParameter>();
        public List<CodegenParameter> bodyParams = new List<CodegenParameter>();
        public List<CodegenParameter> pathParams = new List<CodegenParameter>();
        public List<CodegenParameter> queryParams = new List<CodegenParameter>();
        public List<CodegenParameter> headerParams = new List<CodegenParameter>();
        public List<CodegenParameter> formParams = new List<CodegenParameter>();
        public List<CodegenParameter> requiredParams = new List<CodegenParameter>();
        public List<CodegenSecurity> authMethods;
        public List<Tag> tags;
        public List<CodegenResponse> responses = new List<CodegenResponse>();
        public List<string> imports = new List<string>();
        public List<Dictionary<string, string>> examples;
        public List<Dictionary<string, string>> requestBodyExamples;
        public ExternalDocs externalDocs;
        public Dictionary<string, object> vendorExtensions;
        public string nickname; // legacy support
        public string operationIdLowerCase; // for markdown documentation
        public string operationIdCamelCase; // for class names
        public string operationIdSnakeCase;

        /**
         * Check if there's at least one parameter
         *
         * @return true if parameter exists, false otherwise
         */
        private static bool nonempty<T>(List<T> list) => list != null && list.Count > 0;

        /**
         * Check if there's at least one body parameter
         *
         * @return true if body parameter exists, false otherwise
         */
        public bool getHasBodyParam() => nonempty(bodyParams);

        /**
         * Check if there's at least one query parameter
         *
         * @return true if query parameter exists, false otherwise
         */
        public bool getHasQueryParams() => nonempty(queryParams);

        /**
         * Check if there's at least one header parameter
         *
         * @return true if header parameter exists, false otherwise
         */
        public bool getHasHeaderParams() => nonempty(headerParams);

        /**
         * Check if there's at least one path parameter
         *
         * @return true if path parameter exists, false otherwise
         */
        public bool getHasPathParams() => nonempty(pathParams);

        /**
         * Check if there's at least one form parameter
         *
         * @return true if any form parameter exists, false otherwise
         */
        public bool getHasFormParams() => nonempty(formParams);

        /**
         * Check if there's at least one example parameter
         *
         * @return true if examples parameter exists, false otherwise
         */
        public bool getHasExamples() => nonempty(examples);

        /**
         * Check if act as Restful index method
         *
         * @return true if act as Restful index method, false otherwise
         */
        public bool IsRestfulIndex() => string.Compare(httpMethod, "GET", StringComparison.InvariantCultureIgnoreCase) == 0 && PathWithoutBaseName() == string.Empty;

        /**
         * Check if act as Restful show method
         *
         * @return true if act as Restful show method, false otherwise
         */
        public bool IsRestfulShow() => string.Compare(httpMethod, "GET", StringComparison.InvariantCultureIgnoreCase) == 0 && IsMemberPath();

        /**
         * Check if act as Restful create method
         *
         * @return true if act as Restful create method, false otherwise
         */
        public bool IsRestfulCreate() => string.Compare(httpMethod, "POST", StringComparison.InvariantCultureIgnoreCase) == 0 && PathWithoutBaseName() == string.Empty;

        /**
         * Check if act as Restful update method
         *
         * @return true if act as Restful update method, false otherwise
         */
        public bool IsRestfulUpdate() => new[] { "PUT", "PATCH" }.Contains(httpMethod.ToUpper()) && IsMemberPath();

        /**
         * Check if body param is allowed for the request method
         *
         * @return true request method is PUT, PATCH or POST; false otherwise
         */
        public bool IsBodyAllowed() => new[] { "PUT", "PATCH", "POST" }.Contains(httpMethod.ToUpper());

        /**
         * Check if act as Restful destroy method
         *
         * @return true if act as Restful destroy method, false otherwise
         */
        public bool IsRestfulDestroy() => string.Compare(httpMethod, "DELETE", StringComparison.InvariantCultureIgnoreCase) == 0 && IsMemberPath();

        /**
         * Check if Restful-style
         *
         * @return true if Restful-style, false otherwise
         */
        public bool IsRestful() => IsRestfulIndex() || IsRestfulShow() || IsRestfulCreate() || IsRestfulUpdate() || IsRestfulDestroy();

        /**
         * Get the substring except baseName from path
         *
         * @return the substring
         */
        private string PathWithoutBaseName() => baseName != null ? path.Replace("/" + baseName.ToLower(), string.Empty) : path;

        /**
         * Check if the path match format /xxx/:id
         *
         * @return true if path act as member
         */
        private bool IsMemberPath()
        {
            if (pathParams?.Count != 1) return false;
            string id = pathParams[0].baseName;
            return ("/{" + id + "}") == PathWithoutBaseName();
        }

        public override string ToString() => $"{baseName}({path})";
    }
}
