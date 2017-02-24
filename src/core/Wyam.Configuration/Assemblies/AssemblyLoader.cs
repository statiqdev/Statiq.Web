using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ConcurrentCollections;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
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
        private readonly AssemblyResolver _assemblyResolver;
        private readonly IDirectory _entryAssemblyDirectory;

        private readonly AssemblyCollection _assemblyCollection = new AssemblyCollection();

        private bool _loaded;

        /// <summary>
        /// Gets the assemblies that were directly referenced (as opposed to all recursively referenced assemblies).
        /// </summary>
        public ConcurrentHashSet<Assembly> DirectAssemblies { get; } = new ConcurrentHashSet<Assembly>();

        internal AssemblyLoader(IReadOnlyFileSystem fileSystem, AssemblyResolver assemblyResolver)
        {
            _fileSystem = fileSystem;
            _assemblyResolver = assemblyResolver;

            // Get the location of the entry assembly
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            DirectoryPath entryAssemblyPath = entryAssemblyLocation == null
                ? new DirectoryPath(Environment.CurrentDirectory)
                : new FilePath(entryAssemblyLocation).Directory;
            _entryAssemblyDirectory = _fileSystem.GetDirectory(entryAssemblyPath);

            // Add the Core modules
            DirectAssemblies.Add(Assembly.GetAssembly(typeof(Engine)));
        }

        public void Add(string assembly)
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }

            // Consider it a pattern if it contains a wildcard
            if (assembly.Contains("*"))
            {
                _patterns.Add(assembly);
            }
            else
            {
                _assemblies.Add(assembly);
            }
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
            foreach(string assemblyDeclaration in _assemblies)
            {
                string assembly = assemblyDeclaration.Trim().Trim('"');
                if (assembly.EndsWith(".dll") || assembly.EndsWith(".exe"))
                {
                    // If the path ends with .dll or .exe, attempt to load it as a path
                    LoadAssemblyFromPath(assembly);
                }
                else
                {
                    // Attempt to load as a full name first
                    if (LoadAssemblyFromFullName(assembly) == null)
                    {
                        LoadAssemblyFromSimpleName(assembly);
                    }
                }
            }
        }

        private void LoadAssemblyFromPath(string path)
        {
            FilePath filePath = new FilePath(path);

            // Attempt to load directly, and we're done if it's absolute regardless
            if (LoadAssemblyFromFile(filePath.FullPath) != null || filePath.IsAbsolute)
            {
                return;
            }

            // Attempt to load from the entry assembly path
            if (LoadAssemblyFromFile(_entryAssemblyDirectory.Path.CombineFile(filePath).FullPath) != null)
            {
                return;
            }

            // Attempt to load from the build root
            LoadAssemblyFromFile(_fileSystem.RootPath.CombineFile(filePath).FullPath);
        }

        private Assembly LoadAssemblyFromFile(string assemblyFile)
        {
            using (Trace.WithIndent().Verbose($"Loading assembly file {assemblyFile}"))
            {

                // First load the name so we can check if it's already loaded
                AssemblyName assemblyName = null;
                try
                {
                    assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"{ex.GetType().Name} exception while getting assembly name from file {assemblyFile}: {ex.Message}");
                }

                // If we didn't get a name, then it's not a valid assembly file
                if (assemblyName == null)
                {
                    return null;
                }

                // Attempt to load by name first
                Assembly assembly = LoadAssemblyFromFullName(assemblyName.FullName);
                if (assembly != null)
                {
                    if (assembly.FullName != assemblyName.FullName)
                    {
                        Trace.Verbose($"Assembly {assemblyName} redirected to {assembly.FullName}");
                    }
                    return assembly;
                }
                Trace.Verbose($"Assembly {assemblyName.FullName} could not be loaded by full name, attempting to load by file");

                // Check if the assembly has already been loaded, then attempt to load it if it hasn't
                if (_assemblyResolver.TryGet(assemblyName.FullName, out assembly))
                {
                    Trace.Verbose($"Assembly {assemblyName.FullName} has already been loaded from {assembly.Location}");
                }
                else
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(assemblyFile);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose($"{ex.GetType().Name} exception while loading assembly from file {assemblyFile}: {ex.Message}");
                    }
                }

                ProcessLoadedAssembly(assembly, true);
                return assembly;
            }
        }

        private Assembly LoadAssemblyFromFullName(string name)
        {
            using (Trace.WithIndent().Verbose($"Loading assembly {name} by full name"))
            {
                Assembly assembly = null;
                if (_assemblyCollection.TryGetAssembly(name, out assembly))
                {
                    Trace.Verbose($"Assembly {name} has already been loaded from {assembly.Location}");
                }
                else
                {
                    try
                    {
                        assembly = Assembly.Load(name);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose($"{ex.GetType().Name} exception while loading assembly {name} by full name: {ex.Message}");
                        return null;
                    }
                    ProcessLoadedAssembly(assembly, true);
                }
                return assembly;
            }
        }

        private void LoadAssemblyFromSimpleName(string name)
        {
            // Current algorithm is pretty hacky, take the assembly simple name and use the same version, etc. as a known framework library
            string fullName = typeof(object).Assembly.FullName;
            int firstSpace = fullName.IndexOf(",", StringComparison.Ordinal);  // a comma follows the short name
            if (firstSpace != -1)
            {
                fullName = name + fullName.Substring(firstSpace);
                Trace.Verbose($"Loading assembly {name} as full name {fullName}");
                LoadAssemblyFromFullName(fullName);
            }
        }

        private void ProcessLoadedAssembly(Assembly assembly, bool direct)
        {
            if (assembly != null)
            {
                // Even though we probably already checked, be sure the assembly is in the collection
                if (_assemblyCollection.Add(assembly))
                {
                    Trace.Verbose($"Added assembly {assembly.FullName} from {assembly.Location} to the collection");
                }

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
            // Need to keep track of the assemblies we've seen since assembly binding redirects may result in a different loaded assembly for the input name
            HashSet<string> loadedAssemblies = new HashSet<string>();

            string assemblyName;
            while (_referencedAssemblyNames.TryDequeue(out assemblyName))
            {
                if (!loadedAssemblies.Contains(assemblyName) && !_assemblyCollection.ContainsFullName(assemblyName))
                {
                    loadedAssemblies.Add(assemblyName);
                    using (Trace.WithIndent().Verbose($"Loading referenced assembly {assemblyName}"))
                    {
                        try
                        {
                            Assembly assembly = Assembly.Load(assemblyName);
                            if (assembly != null && assembly.FullName != assemblyName)
                            {
                                Trace.Verbose($"Assembly {assemblyName} redirected to {assembly.FullName}");
                            }
                            ProcessLoadedAssembly(assembly, false);
                        }
                        catch (Exception ex)
                        {
                            Trace.Verbose($"{ex.GetType().Name} exception while loading referenced assembly {assemblyName}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}