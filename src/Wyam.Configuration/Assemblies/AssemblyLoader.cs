using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Configuration.ConfigScript;
using Wyam.Core.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Configuration.Assemblies
{
    public class AssemblyLoader : IDisposable
    {
        private readonly ConcurrentHashSet<string> _patterns = new ConcurrentHashSet<string>();
        private readonly ConcurrentHashSet<string> _names = new ConcurrentHashSet<string>();
        private readonly ConcurrentHashSet<string> _loadedAssemblyNames = new ConcurrentHashSet<string>();

        private readonly ConfigCompilation _compilation;
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly IAssemblyCollection _assemblies;

        private bool _loaded;
        private bool _disposed;

        /// <summary>
        /// Gets the assemblies that were directly referenced (as opposed to all recursively referenced assemblies).
        /// </summary>
        public ConcurrentHashSet<Assembly> DirectAssemblies { get; } = new ConcurrentHashSet<Assembly>();

        internal AssemblyLoader(ConfigCompilation compilation, IReadOnlyFileSystem fileSystem, IAssemblyCollection assemblies)
        {
            _compilation = compilation;
            _fileSystem = fileSystem;
            _assemblies = assemblies;

            // Add the Core modules
            DirectAssemblies.Add(Assembly.GetAssembly(typeof(Engine)));

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = string.Empty; // non-null means exclude application base path
        }
        
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            _disposed = true;
        }

        public void AddPattern(string pattern) => _patterns.Add(pattern);

        public void AddName(string name) => _names.Add(name);

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        internal void LoadAssemblies()
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }
            _loaded = true;

            LoadAssembliesByPath();
            LoadAssembliesByName();
        }

        private void LoadAssembliesByPath()
        {
            // Get path to all assemblies (except those specified by name)
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            DirectoryPath entryAssemblyPath = entryAssemblyLocation == null
                ? new DirectoryPath(Environment.CurrentDirectory)
                : new FilePath(entryAssemblyLocation).Directory; 
            IDirectory entryAssemblyDirectory = _fileSystem.GetDirectory(entryAssemblyPath);

            // Get the assemblies local to the entry assembly
            List<FilePath> assemblyPaths = _fileSystem
                .GetFiles(entryAssemblyDirectory, _patterns)
                .Where(x => x.Path.Extension == ".dll" || x.Path.Extension == ".exe")
                .Select(x => x.Path)
                .ToList();

            // Get requested assemblies from the build root
            assemblyPaths.AddRange(_fileSystem
                .GetFiles(_patterns)
                .Where(x => x.Path.Extension == ".dll" || x.Path.Extension == ".exe")
                .Select(x => x.Path));

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";",
                    new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => x.Directory.FullPath).Distinct())
                    .Distinct());

            // Load the assemblies by path
            Parallel.ForEach(assemblyPaths.Select(x => x.FullPath).Distinct(), assemblyPath =>
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                if (_loadedAssemblyNames.Add(assemblyName.ToString()))
                {
                    Trace.Verbose("Loading assembly file {0}", assemblyPath);

                    // Load the assembly
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                    }

                    // Add the assembly and load references
                    if (assembly != null)
                    {
                        DirectAssemblies.Add(assembly);
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
            });
        }

        private void LoadAssembliesByName()
        {
            Parallel.ForEach(_names, assemblyName =>
            {
                if (_loadedAssemblyNames.Add(assemblyName))
                {
                    Trace.Verbose("Loading assembly {0}", assemblyName);

                    // Load the assembly
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                    }

                    // Add the assembly and load references
                    if (assembly != null)
                    {
                        DirectAssemblies.Add(assembly);
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
            });
        }

        // We need to go ahead and load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private void LoadReferencedAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            Parallel.ForEach(assemblyNames.Where(x => !_assemblies.ContainsFullName(x.FullName)), assemblyName =>
            {
                try
                {
                    if (_loadedAssemblyNames.Add(assemblyName.ToString()))
                    {
                        Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                        Assembly assembly = Assembly.Load(assemblyName);
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            });
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args) => _assemblies.Add(args.LoadedAssembly);

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Only start resolving after we've generated the config assembly
            if (_compilation.Assembly != null && args.Name == _compilation.AssemblyFullName)
            {
                // Return the dynamically compiled config assembly if given it's name
                return _compilation.Assembly;
            }

            // Return an assembly from the cache
            Assembly assembly;
            return _assemblies.TryGetAssembly(args.Name, out assembly) ? assembly : null;
        }
    }
}