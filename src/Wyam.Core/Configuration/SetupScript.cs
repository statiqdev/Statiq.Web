using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.NuGet;

namespace Wyam.Core.Configuration
{
    internal class SetupScript
    {
        public const string AssemblyName = "WyamSetup";

        public string Code { get; private set; }

        public byte[] RawAssembly { get; private set; }

        public Assembly Assembly { get; private set; }

        public SetupScript(ConfigParts configParts)
        {
            Code = Generate(configParts);
        }

        private static string Generate(ConfigParts configParts)
        {
            StringBuilder builder = new StringBuilder(@"
                        using System;
                        using Wyam.Common.Configuration;
                        using Wyam.Common.IO;
                        using Wyam.Common.NuGet;");
            builder.AppendLine();
            builder.AppendLine(string.Join(Environment.NewLine,
                typeof(IModule).Assembly.GetTypes()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                    .Select(x => "using " + x.Namespace + ";")
                    .Distinct()));
            builder.AppendLine(@"
                        public static class SetupScript
                        {
                            public static void Run(IPackagesCollection Packages, IAssemblyCollection Assemblies, IConfigurableFileSystem FileSystem)
                            {");
            builder.Append(configParts.Setup);
            builder.AppendLine(@"
                            }
                        }");
            return builder.ToString();
        }

        public void Compile()
        {
            // Assemblies
            Assembly[] referenceAssemblies =
            {
                Assembly.GetAssembly(typeof (object)), // System
                Assembly.GetAssembly(typeof (Engine)), //Wyam.Core
                Assembly.GetAssembly(typeof (IModule)) // Wyam.Common
            };

            // Load the dynamic assembly and invoke
            RawAssembly = ConfigCompiler.Compile(AssemblyName, referenceAssemblies, Code);
            Assembly = Assembly.Load(RawAssembly);
        }

        public void Invoke(IPackagesCollection packages, IAssemblyCollection assemblies, IConfigurableFileSystem fileSystem)
        {
            var scriptType = Assembly.GetExportedTypes().First(t => t.Name == "SetupScript");
            MethodInfo runMethod = scriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
            runMethod.Invoke(null, new object[] { packages, assemblies, fileSystem });
        }
    }
}
