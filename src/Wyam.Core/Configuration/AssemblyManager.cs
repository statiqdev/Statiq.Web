using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Core.NuGet;
using Wyam.Core.Util;

namespace Wyam.Core.Configuration
{
    internal class AssemblyManager
    {
        private static readonly string[] DefaultNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.IO",
            "Wyam.Core",
            "Wyam.Core.Configuration"
        };

        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _namespaces = new HashSet<string>();

        public IEnumerable<Assembly> Assemblies => _assemblies.Values;

        public IEnumerable<string> Namespaces => _namespaces;

        public AssemblyManager()
        {
            // This is the default set of assemblies that should get loaded during configuration and in other dynamic modules
            AddAssembly(Assembly.GetAssembly(typeof(object))); // System
            AddAssembly(Assembly.GetAssembly(typeof(System.Collections.Generic.List<>))); // System.Collections.Generic 
            AddAssembly(Assembly.GetAssembly(typeof(System.Linq.ImmutableArrayExtensions))); // System.Linq
            AddAssembly(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject))); // System.Core (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo))); // Microsoft.CSharp (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof(System.IO.Stream))); // System.IO
            AddAssembly(Assembly.GetAssembly(typeof(System.Diagnostics.TraceSource))); // System.Diagnostics
            AddAssembly(Assembly.GetAssembly(typeof(Wyam.Core.Engine))); // Wyam.Core
            AddAssembly(Assembly.GetAssembly(typeof(IModule))); // Wyam.Common

            // Add the default set of namespaces
            _namespaces.AddRange(DefaultNamespaces);
        }

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        public void Initialize(AssemblyCollection assemblyCollection, PackagesCollection packages, IFileSystem fileSystem)
        {
            // Add all module namespaces from Wyam.Core
            _namespaces.AddRange(typeof(Engine).Assembly.GetTypes()
                .Where(x => typeof(IModule).IsAssignableFrom(x))
                .Select(x => x.Namespace));

            // Also include all Wyam.Common namespaces
            _namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace));

            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            assemblyPaths.AddRange(packages.GetCompatibleAssemblyPaths());
            assemblyPaths.AddRange(Directory.GetFiles(System.IO.Path.GetDirectoryName(typeof(Config).Assembly.Location), "*.dll", SearchOption.AllDirectories));
            assemblyPaths.AddRange(assemblyCollection.Directories
                .Select(x => new Tuple<string, SearchOption>(System.IO.Path.Combine(fileSystem.RootPath.FullPath, x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1))
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(assemblyCollection.ByFile
                .Select(x => new Tuple<string, string>(x, System.IO.Path.Combine(fileSystem.RootPath.FullPath, x)))
                .Select(x => File.Exists(x.Item2) ? x.Item2 : x.Item1));

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";", new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => System.IO.Path.GetDirectoryName(x).Distinct())));

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            // If this approach causes problems, could also try loading assemblies in custom app domain:
            // http://stackoverflow.com/questions/6626647/custom-appdomain-and-privatebinpath
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    Trace.Verbose("Loading assembly file {0}", assemblyPath);
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!AddAssembly(assembly))
                    {
                        Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in assemblyCollection.ByName)
            {
                try
                {
                    Trace.Verbose("Loading assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!AddAssembly(assembly))
                    {
                        Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private void LoadReferencedAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames.Where(x => !_assemblies.ContainsKey(x.FullName)))
            {
                try
                {
                    Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    AddAssembly(assembly);
                    LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }

            }
        }

        public bool AddAssembly(Assembly assembly)
        {
            if (_assemblies.ContainsKey(assembly.FullName))
            {
                return false;
            }
            _assemblies.Add(assembly.FullName, assembly);
            return true;
        }

        public HashSet<Type> GetModuleTypes()
        {
            HashSet<Type> moduleTypes = new HashSet<Type>();
            foreach (Assembly assembly in _assemblies.Values)
            {
                using (Trace.WithIndent().Verbose("Searching for modules in assembly {0}", assembly.FullName))
                {
                    foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                        && x.IsPublic && !x.IsAbstract && x.IsClass))
                    {
                        Trace.Verbose("Found module {0} in assembly {1}", moduleType.Name, assembly.FullName);
                        moduleTypes.Add(moduleType);
                        _namespaces.Add(moduleType.Namespace);
                    }
                }
            }
            return moduleTypes;
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    Trace.Verbose("Loader Exception: {0}", loaderException.Message);
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }

        public bool TryGetAssembly(string fullName, out Assembly assembly)
        {
            return _assemblies.TryGetValue(fullName, out assembly);
        }
    }
}