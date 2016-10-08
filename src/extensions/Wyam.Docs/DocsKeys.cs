using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Docs
{
    public static class DocsKeys
    {
        /// <summary>
        /// The location of your source files. Can be a <c>string</c>
        /// or a collection of <c>string</c>. This will be evaluated
        /// from the context of the input virtual file system.
        /// </summary>
        public const string SourceFiles = nameof(SourceFiles);
        public const string IncludeGlobal = nameof(IncludeGlobal);
        public const string ApiPathPrefix = nameof(ApiPathPrefix);
        public const string Title = nameof(Title);
        public const string NoSidebar = nameof(NoSidebar);
        public const string NoContainer = nameof(NoContainer);
        public const string NoGutter = nameof(NoGutter);
    }
}
