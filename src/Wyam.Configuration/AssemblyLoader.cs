using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Configuration.NuGet;
using Wyam.Core;
using Wyam.Core.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Configuration
{
    internal class AssemblyLoader : IDisposable
    {
        // TODO: Stop loading all referenced assemblies (I.e., we don't need to load all the nuget assemblies)

        // TODO: Store the added globbing patterns in a concurrent safe hash set
        private readonly List<Tuple<DirectoryPath, SearchOption>> _directories = new List<Tuple<DirectoryPath, SearchOption>>();
        private readonly List<FilePath> _byFile = new List<FilePath>();
        private readonly List<string> _byName = new List<string>();
        private readonly HashSet<Type> _moduleTypes = new HashSet<Type>();

        private readonly ConfigCompilation _compilation;
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly IAssemblyCollection _assemblies;
        private readonly INamespacesCollection _namespaces;

        private bool _disposed;

        public AssemblyLoader(ConfigCompilation compilation, IReadOnlyFileSystem fileSystem, IAssemblyCollection assemblies, INamespacesCollection namespaces)
        {
            _compilation = compilation;
            _fileSystem = fileSystem;
            _assemblies = assemblies;
            _namespaces = namespaces;

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

        //TODO: Change LoadDirectory and LoadFile to a single method that loads assemblies based on a globbing pattern
        public void AddDirectory(DirectoryPath path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            _directories.Add(new Tuple<DirectoryPath, SearchOption>(path, searchOption));
        }

        public void AddFile(FilePath path)
        {
            _byFile.Add(path);
        }

        public void AddName(string name)
        {
            _byName.Add(name);
        }

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        public void LoadAssemblies()
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
            List<FilePath> assemblyPaths = new List<FilePath>();
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            if (entryAssemblyLocation != null)
            {
                assemblyPaths.AddRange(Directory
                    .GetFiles(new FilePath(entryAssemblyLocation).Directory.FullPath, "*.dll", SearchOption.AllDirectories)
                    .Select(x => new FilePath(x)));
            }
            assemblyPaths.AddRange(_directories
                .Select(x => new Tuple<DirectoryPath, SearchOption>(_fileSystem.RootPath.Combine(x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1.FullPath))
                .SelectMany(x => Directory.GetFiles(x.Item1.FullPath, "*.dll", x.Item2).Select(y => new FilePath(y))));
            assemblyPaths.AddRange(_byFile
                .Select(x => new Tuple<FilePath, FilePath>(x, _fileSystem.RootPath.CombineFile(x)))
                .Select(x => File.Exists(x.Item2.FullPath) ? x.Item2 : x.Item1));

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";", 
                    new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => x.Directory.FullPath).Distinct())
                    .Distinct());

            // Keep track of references assemblies, but don't load them yet until all other assemblies are loaded
            HashSet<AssemblyName> referencedAssemblyNames = new HashSet<AssemblyName>();

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            // If this approach causes problems, could also try loading assemblies in custom app domain:
            // http://stackoverflow.com/questions/6626647/custom-appdomain-and-privatebinpath
            foreach (string assemblyPath in assemblyPaths.Select(x => x.FullPath).Distinct())
            {
                try
                {
                    Trace.Verbose("Loading assembly file {0}", assemblyPath);
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!_assemblies.Add(assembly))
                    {
                        Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                    }
                    else
                    {
                        referencedAssemblyNames.AddRange(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in _byName)
            {
                try
                {
                    Trace.Verbose("Loading assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!_assemblies.Add(assembly))
                    {
                        Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                    }
                    else
                    {
                        referencedAssemblyNames.AddRange(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }

            // Load any referenced assemblies
            LoadReferencedAssemblies(_assemblies, referencedAssemblyNames);

            // Scan for required types
            FindModuleTypes(_assemblies, _namespaces);
        }

        // We need to go ahead and load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private static void LoadReferencedAssemblies(IAssemblyCollection assemblies, IEnumerable<AssemblyName> assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames.Where(x => !assemblies.ContainsFullName(x.FullName)))
            {
                try
                {
                    Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    assemblies.Add(assembly);
                    LoadReferencedAssemblies(assemblies, assembly.GetReferencedAssemblies());
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private void FindModuleTypes(IAssemblyCollection assemblies, INamespacesCollection namespaces)
        {
            foreach (Assembly assembly in assemblies)
            {
                using (Trace.WithIndent().Verbose("Searching for modules in assembly {0}", assembly.FullName))
                {
                    foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                        && x.IsPublic && !x.IsAbstract && x.IsClass))
                    {
                        Trace.Verbose("Found module {0} in assembly {1}", moduleType.Name, assembly.FullName);
                        _moduleTypes.Add(moduleType);
                        namespaces.Add(moduleType.Namespace);
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