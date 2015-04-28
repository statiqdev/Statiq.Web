using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using NuGet;
using Wyam.Core.Extensibility;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Configurator
    {
        private readonly Engine _engine;
        private readonly PackagesCollection _packages = new PackagesCollection();
        private readonly AssemblyCollection _assemblies = new AssemblyCollection();
        private readonly NamespacesCollection _namespaces = new NamespacesCollection();

        public Configurator(Engine engine)
        {
            _engine = engine;
        }

        // Preconfig is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script = null)
        {
            // Default metadata is configured regardless of if a config script has been provided
            // The script can overwrite or clear the default metadata if needed
            ConfigureDefaultMetadata();

            // If no script, nothing else to do
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            Tuple<string, string> configParts = GetConfigParts(script);
            if (!string.IsNullOrWhiteSpace(configParts.Item1))
            {
                // Preconfigure (install packages, specify additional assemblies and namespaces, etc.)
                Preconfig(configParts.Item1);
                _packages.InstallPackages();
            }
            Config(configParts.Item2);
        }

        // Item1 = preconfig (possibly null), Item2 = config
        public Tuple<string, string> GetConfigParts(string script)
        {
            string preconfig = null;
            List<string> configLines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int preconfigLine = configLines.FindIndex(x => x.Trim().All(y => y == '-'));
            if (preconfigLine != -1)
            {
                preconfig = string.Join(Environment.NewLine, configLines.Take(preconfigLine));
                configLines.RemoveRange(0, preconfigLine + 1);
            }
            return new Tuple<string, string>(preconfig, string.Join(Environment.NewLine, configLines));
        }

        private void Preconfig(string script)
        {
            try
            {
                // Create the script options
                ScriptOptions scriptOptions = new ScriptOptions()
                    .AddNamespaces(
                        "System",
                        "Wyam.Core",
                        "Wyam.Core.Extensibility")
                    .AddReferences(
                        Assembly.GetAssembly(typeof(object)),  // System
                        Assembly.GetAssembly(typeof(Engine)));  // Wyam.Core

                // Evaluate the script
                CSharpScript.Eval(script, scriptOptions, new PreConfigGlobals(_packages, _assemblies, _namespaces));
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.Error("Error compiling pre-configuration: {0}", compilationError.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during pre-configuration: {0}", ex.ToString());
                throw;
            }
        }

        private void Config(string script)
        {
            try
            {
                // Create the script options
                ScriptOptions scriptOptions = new ScriptOptions()
                    .AddNamespaces(
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.IO",
                        "Wyam.Core",
                        "Wyam.Core.Extensibility",
                        "Wyam.Core.Modules",
                        "Wyam.Core.Helpers")
                    .AddReferences(
                        Assembly.GetAssembly(typeof(object)),  // System
                        Assembly.GetAssembly(typeof(List<>)),  // System.Collections.Generic 
                        Assembly.GetAssembly(typeof(ImmutableArrayExtensions)),  // System.Linq
                        Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Core (needed for dynamic)
                        Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp (needed for dynamic)
                        Assembly.GetAssembly(typeof(Path)), // System.IO
                        Assembly.GetAssembly(typeof(Engine)));  // Wyam.Core

                // Add specified assemblies and find modules from packages, etc.
                HashSet<Type> moduleTypes;
                scriptOptions = AddAssembliesAndFindModules(scriptOptions, out moduleTypes);

                // Add our own modules to the list of module types
                moduleTypes.AddRange(GetModuleTypes(typeof(Engine).Assembly));

                // Add any additional namespaces
                scriptOptions = scriptOptions.AddNamespaces(_namespaces.Ns);

                // Generate the script
                script = GenerateScript(script, moduleTypes);

                // Evaluate the script
                CSharpScript.Eval(script, scriptOptions, new ConfigGlobals(_engine.Metadata, _engine.Pipelines));
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.Error("Error compiling configuration: {0}", compilationError.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during configuration: {0}", ex.ToString());
                throw;
            }
        }

        // This creates a wrapper class for the config script that contains static methods for constructing modules
        private string GenerateScript(string script, HashSet<Type> moduleTypes)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.Append(@"
                public static class ConfigScript
                {
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines)
                    {
                        " + script + @"
                    }");

            // Add static methods to construct each module
            // Use Roslyn to get a display string for each constructor
            foreach (Type moduleType in moduleTypes)
            {
                CSharpCompilation compilation = CSharpCompilation
                    .Create("ScriptCtorMethodGen")
                    .AddReferences(MetadataReference.CreateFromAssembly(moduleType.Assembly));
                foreach (AssemblyName referencedAssembly in moduleType.Assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        compilation = compilation.AddReferences(
                            MetadataReference.CreateFromAssembly(Assembly.Load(referencedAssembly)));
                    }
                    catch (Exception)
                    {
                        // We don't care about problems loading referenced assemblies, just ignore them
                    }
                }
                INamedTypeSymbol moduleSymbol = compilation.GetTypeByMetadataName(moduleType.FullName);
                foreach (IMethodSymbol ctorSymbol in moduleSymbol.InstanceConstructors
                    .Where(x => x.DeclaredAccessibility == Accessibility.Public))
                {
                    string ctorDisplayString = ctorSymbol.ToDisplayString(new SymbolDisplayFormat(
                        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                        parameterOptions: SymbolDisplayParameterOptions.IncludeName
                            | SymbolDisplayParameterOptions.IncludeDefaultValue
                            | SymbolDisplayParameterOptions.IncludeParamsRefOut
                            | SymbolDisplayParameterOptions.IncludeType,
                        memberOptions: SymbolDisplayMemberOptions.IncludeParameters
                            | SymbolDisplayMemberOptions.IncludeContainingType,
                        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
                    string ctorCallDisplayString = 	ctorSymbol.ToDisplayString(new SymbolDisplayFormat(
                        parameterOptions: SymbolDisplayParameterOptions.IncludeName 
		                    | SymbolDisplayParameterOptions.IncludeParamsRefOut,
		                memberOptions: SymbolDisplayMemberOptions.IncludeParameters));
                    scriptBuilder.AppendFormat(@"
                        public static {0} {1}{2}
                        {{
                            return new {0}{3};  
                        }}",
                        moduleType.FullName,
                        moduleType.Name,
                        ctorDisplayString.Substring(ctorDisplayString.IndexOf("(", StringComparison.Ordinal)),
                        ctorCallDisplayString.Substring(ctorCallDisplayString.IndexOf("(", StringComparison.Ordinal)));
                }
            }

            scriptBuilder.Append(@"
                }

                ConfigScript.Run(Metadata, Pipelines);");
            return scriptBuilder.ToString();
        }

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        private ScriptOptions AddAssembliesAndFindModules(ScriptOptions scriptOptions, out HashSet<Type> moduleTypes)
        {
            moduleTypes = new HashSet<Type>();

            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            if (Directory.Exists(_packages.Path))
            {
                assemblyPaths.AddRange(Directory.GetFiles(_packages.Path, "*.dll", SearchOption.AllDirectories));
            }
            assemblyPaths.AddRange(_assemblies.Directories
                .Where(x => Directory.Exists(x.Item1))
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(_assemblies.ByPath.Where(File.Exists));

            // Iterate assemblies by path, add them to the script, and check for modules
            List<Assembly> assemblies = new List<Assembly>();
            HashSet<string> namespaces = new HashSet<string>();
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    assemblies.Add(assembly);
                    foreach (Type moduleType in GetModuleTypes(assembly))
                    {
                        moduleTypes.Add(moduleType);
                        namespaces.Add(moduleType.Namespace);
                    }
                }
                catch (FileLoadException)
                {
                    // The Assembly has already been loaded
                }
                catch (BadImageFormatException)
                {
                    // If a BadImageFormatException exception is thrown, the file is not an assembly
                }
                catch (Exception ex)
                {
                    // Some other reason the assembly couldn't be loaded or we couldn't reflect
                    _engine.Trace.Verbose("Unexpected exception while loading assembly at {0}: {1}.", assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in _assemblies.ByName)
            {
                try
                {
                    Assembly assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                    foreach (Type moduleType in GetModuleTypes(assembly))
                    {
                        moduleTypes.Add(moduleType);
                        namespaces.Add(moduleType.Namespace);
                    }
                }
                catch (FileLoadException)
                {
                    // The Assembly has already been loaded
                }
                catch (BadImageFormatException)
                {
                    // If a BadImageFormatException exception is thrown, the file is not an assembly
                }
                catch (Exception ex)
                {
                    // Some other reason the assembly couldn't be loaded or we couldn't reflect
                    _engine.Trace.Verbose("Unexpected exception while loading assembly {0}: {1}.", assemblyName, ex.Message);
                }
            }

            return scriptOptions
                .AddNamespaces(namespaces)
                .AddReferences(assemblies);
        }

        private IEnumerable<Type> GetModuleTypes(Assembly assembly)
        {
            return assembly.GetTypes().Where(x => typeof(IModule).IsAssignableFrom(x)
                && x.IsPublic && !x.IsAbstract && x.IsClass && !x.ContainsGenericParameters);
        }

        private void ConfigureDefaultMetadata()
        {
            _engine.Metadata["InputPath"] = @".\input";
            _engine.Metadata["OutputPath"] = @".\output";
        }

        public void ConfigureDefaultPipelines()
        {
            // TODO: Call this from the console project if no script is specified - if using the engine directly, default pipelines must be explicitly configured
        }
    }
}
