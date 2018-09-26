using System.Collections.Generic;

namespace Wyam.SwaggerCodeGen
{
    public class CodegenSecurity
    {
        public string name;
        public string type;
        public bool hasMore;
        public bool isBasic;
        public bool isOAuth;
        public bool isApiKey;
        // ApiKey specific
        public string keyParamName;
        public bool isKeyInQuery;
        public bool isKeyInHeader;
        // Oauth specific
        public string flow;
        // Oauth specific
        public string authorizationUrl;
        // Oauth specific
        public string tokenUrl;
        public List<Dictionary<string, object>> scopes;
        public bool isCode;
        public bool isPassword;
        public bool isApplication;
        public bool isImplicit;

        public override string ToString() => $"{name}({type})";
    }
}