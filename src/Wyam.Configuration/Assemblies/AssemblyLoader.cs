using System;
using System.Collections.Concurrent;
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
    public class AssemblyLoader
    {
        private readonly ConcurrentHashSet<string> _patterns = new ConcurrentHashSet<string>();
        private readonly ConcurrentHashSet<string> _assemblies = new ConcurrentHashSet<string>();
        private readonly ConcurrentQueue<string> _referencedAssemblyNames = new ConcurrentQueue<string>();
        
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly IAssemblyCollection _assemblyCollection;
        private readonly IDirectory _entryAssemblyDirectory;

        private bool _loaded;

        /// <summary>
        /// Gets the assemblies that were directly referenced (as opposed to all recursively referenced assemblies).
        /// </summary>
        public ConcurrentHashSet<Assembly> DirectAssemblies { get; } = new ConcurrentHashSet<Assembly>();

        internal AssemblyLoader(IReadOnlyFileSystem fileSystem, IAssemblyCollection assemblyCollection)
        {
            _fileSystem = fileSystem;
            _assemblyCollection = assemblyCollection;

            // Get the location of the entry assembly
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            DirectoryPath entryAssemblyPath = entryAssemblyLocation == null
                ? new DirectoryPath(Environment.CurrentDirectory)
                : new FilePath(entryAssemblyLocation).Directory;
            _entryAssemblyDirectory = _fileSystem.GetDirectory(entryAssemblyPath);

            // Add the Core modules
            DirectAssemblies.Add(Assembly.GetAssembly(typeof(Engine)));

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = string.Empty; // non-null means exclude application base path
        }

        public void AddPattern(string pattern)
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }
            _patterns.Add(pattern);
        }

        public void AddReference(string name)
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }
            _assemblies.Add(name);
        }

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        internal void Load()
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }
            _loaded = true;

            ProcessPatterns();
            LoadAssemblies();
            LoadReferencedAssemblies();
        }

        private void ProcessPatterns()
        {
            // Get the assemblies local to the entry assembly
            _assemblies.AddRange(_fileSystem
                .GetFiles(_entryAssemblyDirectory, _patterns)
                .Where(x => x.Path.Extension == ".dll" || x.Path.Extension == ".exe")
                .Select(x => x.Path.FullPath));

            // Get requested assemblies from the build root
            _assemblies.AddRange(_fileSystem
                .GetFiles(_patterns)
                .Where(x => x.Path.Extension == ".dll" || x.Path.Extension == ".exe")
                .Select(x => x.Path.FullPath));
        }

        private void LoadAssemblies()
        {
            Parallel.ForEach(_assemblies, assembly =>
            {
                assembly = assembly.Trim().Trim('"');
                if (assembly.EndsWith(".dll") || assembly.EndsWith(".exe"))
                {
                    // If the path ends with .dll or .exe, attempt to load it as a path
                    LoadAssemblyFromPath(assembly);
                }
                else
                {
                    // Attempt to load as a full name first
                    if (!LoadAssemblyFromFullName(assembly))
                    {
                        LoadAssemblyFromSimpleName(assembly);
                    }
                }
            });
        }

        private void LoadAssemblyFromPath(string path)
        {
            FilePath filePath = new FilePath(path);

            // Get the assembly from the entry assembly path (or directly if absolute)
            FilePath loadPath = _entryAssemblyDirectory.Path.CombineFile(filePath);
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(loadPath.FullPath);
            }
            catch (Exception ex)
            {
                Trace.Verbose($"{ex.GetType().Name} exception while loading assembly from {loadPath.FullPath}: {ex.Message}");
            }

            // If we didn't get an assembly, and the original path wasn't absolute, try from the build root
            if (assembly == null && filePath.IsRelative)
            {
                loadPath = _fileSystem.RootPath.CombineFile(filePath);
                try
                {
                    assembly = Assembly.LoadFrom(loadPath.FullPath);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"{ex.GetType().Name} exception while loading assembly from {loadPath.FullPath}: {ex.Message}");
                }
            }

            ProcessLoadedAssembly(assembly, true);
        }

        private bool LoadAssemblyFromFullName(string name)
        {
            if (!_assemblyCollection.ContainsFullName(name))
            {
                Trace.Verbose($"Loading assembly {name} by full name");
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(name);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"{ex.GetType().Name} exception while loading assembly {name} by full name: {ex.Message}");
                    return false;
                }
                ProcessLoadedAssembly(assembly, true);
            }
            return true;
        }

        private void LoadAssemblyFromSimpleName(string name)
        {

        }

        private void ProcessLoadedAssembly(Assembly assembly, bool direct)
        {
            if (assembly != null)
            {
                // Even though we probably already checked, be sure the assembly is in the collection
                _assemblyCollection.Add(assembly);

                // Keep track of assemblies we directly asked for so we can scan them for the class catalog
                if (direct)
                {
                    DirectAssemblies.Add(assembly);
                }

                // Enqueue all referenced assemblies
                foreach (AssemblyName referencedAssemblyName in assembly.GetReferencedAssemblies())
                {
                    _referencedAssemblyNames.Enqueue(referencedAssemblyName.ToString());
                }
            }
        }

        // We need to go ahead and load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private void LoadReferencedAssemblies()
        {
            string assemblyName;
            while (_referencedAssemblyNames.TryDequeue(out assemblyName))
            {
                try
                {
                    if (!_assemblyCollection.ContainsFullName(assemblyName))
                    {
                        Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                        Assembly assembly = Assembly.Load(assemblyName);
                        ProcessLoadedAssembly(assembly, false);
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }

            }
        }
    }
}