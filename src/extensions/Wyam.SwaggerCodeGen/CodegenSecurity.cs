using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
[assembly: SuppressMessage("", "SA1515", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1005", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1508", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1124", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1507", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1132", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1005", Justification = "Stop !")]

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