using System;
using System.Collections.Generic;
using System.Reflection;

using Wyam.Common.Execution;

namespace Wyam.Configuration.ConfigScript
{
    internal interface IScriptManager
    {
        string Code { get; }
        Assembly Assembly { get; }
        string AssemblyFullName { get; }
        byte[] RawAssembly { get; }
        void Compile(IReadOnlyCollection<Assembly> referenceAssemblies);
        void LoadCompiledConfig(byte[] rawAssembly);
        void Evaluate(IEngine engine);
        void Create(string code, IReadOnlyCollection<Type> moduleTypes, IEnumerable<string> namespaces);
    }
}