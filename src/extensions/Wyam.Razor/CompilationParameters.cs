using System;
using Wyam.Common.IO;

namespace Wyam.Razor
{
    internal struct CompilationParameters
    {
        public IReadOnlyFileSystem FileSystem { get; set; }
        public NamespaceCollection Namespaces { get; set; }
        public DynamicAssemblyCollection DynamicAssemblies { get; set; }
        public Type BasePageType { get; set; }
    }
}