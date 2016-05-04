using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Configuration.Assemblies
{
    public class AssemblyLoader : IDisposable
    {
        private readonly ConcurrentHashSet<string> _patterns = new ConcurrentHashSet<string>();
        private readonly ConcurrentHashSet<string> _names = new ConcurrentHashSet<string>();
        private readonly List<Assembly> _moduleAssemblies = new List<Assembly>();
        private readonly HashSet<Type> _moduleTypes = new HashSet<Type>();

        private readonly ConfigCompilation _compilation;
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly IAssemblyCollection _assemblies;
        private readonly INamespacesCollection _namespaces;

        private bool _disposed;

        internal AssemblyLoader(ConfigCompilation compilation, IReadOnlyFileSystem fileSystem, IAssemblyCollection assemblies, INamespacesCollection namespaces)
        {
            _compilation = compilation;
            _fileSystem = fileSystem;
            _assemblies = assemblies;
            _namespaces = namespaces;

            // Add the Core modules
            _moduleAssemblies.Add(Assembly.GetAssembly(typeof(Engine)));

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = string.Empty; // non-null means exclude application base path
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            _disposed = true;
        }

        internal IEnumerable<Type> ModuleTypes => _moduleTypes;

        public void AddPattern(string pattern)
        {
            _patterns.Add(pattern);
        }

        public void AddName(string name)
        {
            _names.Add(name);
        }

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        internal void LoadAssemblies()
        {
            // Add all module namespaces from Wyam.Core
            _namespaces.AddRange(typeof(Engine).Assembly.GetTypes()
                .Where(x => typeof(IModule).IsAssignableFrom(x))
                .Select(x => x.Namespace));

            // Also include all Wyam.Common namespaces
            _namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace));

            LoadAssembliesByPath();
            LoadAssembliesByName();
            FindModuleTypes();
        }

        private void LoadAssembliesByPath()
        {
            // Get path to all assemblies (except those specified by name)
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            DirectoryPath entryAssemblyPath = entryAssemblyLocation == null
                ? new DirectoryPath(Environment.CurrentDirectory)
                : new FilePath(entryAssemblyLocation).Directory; 
            IDirectory entryAssemblyDirectory = _fileSystem.GetDirectory(entryAssemblyPath);
            List<FilePath> assemblyPaths = _fileSystem
                .GetFiles(entryAssemblyDirectory, _patterns)
                .Where(x => x.Path.Extension == ".dll" || x.Path.Extension == ".exe")
                .Select(x => x.Path)
                .ToList();

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";",
                    new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => x.Directory.FullPath).Distinct())
                    .Distinct());

            foreach (string assemblyPath in assemblyPaths.Select(x => x.FullPath).Distinct())
            {
                try
                {
                    using (Trace.WithIndent().Verbose("Loading assembly file {0}", assemblyPath))
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                        Assembly assembly = Assembly.Load(assemblyName);
                        if (!_assemblies.Add(assembly))
                        {
                            Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                        }
                        else
                        {
                            _moduleAssemblies.Add(assembly);
                            LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }
        }

        private void LoadAssembliesByName()
        {
            foreach (string assemblyName in _names)
            {
                try
                {
                    using (Trace.WithIndent().Verbose("Loading assembly {0}", assemblyName))
                    {
                        Assembly assembly = Assembly.Load(assemblyName);
                        if (!_assemblies.Add(assembly))
                        {
                            Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                        }
                        else
                        {
                            _moduleAssemblies.Add(assembly);
                            LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        // We need to go ahead and load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private void LoadReferencedAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames.Where(x => !_assemblies.ContainsFullName(x.FullName)))
            {
                try
                {
                    using (Trace.WithIndent().Verbose("Loading referenced assembly {0}", assemblyName))
                    {
                        Assembly assembly = Assembly.Load(assemblyName);
                        _assemblies.Add(assembly);
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private void FindModuleTypes()
        {
            foreach (Assembly assembly in _moduleAssemblies)
            {
                using (Trace.WithIndent().Verbose("Searching for modules in assembly {0}", assembly.FullName))
                {
                    foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                        && x.IsPublic && !x.IsAbstract && x.IsClass))
                    {
                        Trace.Verbose("Found module {0} in assembly {1}", moduleType.Name, assembly.FullName);
                        _moduleTypes.Add(moduleType);
                        _namespaces.Add(moduleType.Namespace);
                    }
                }
            }
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

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Only start resolving after we've generated the config assembly
            if (_compilation.Assembly != null)
            {
                // Return the dynamically compiled config assembly if given it's name
                if (args.Name == _compilation.AssemblyFullName)
                {
                    return _compilation.Assembly;
                }

                // Return an assembly from the cache
                Assembly assembly;
                if (_assemblies.TryGetAssembly(args.Name, out assembly))
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}