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

        // First = referenced assembly name, Second = referencing assembly (for locating the referenced assembly)
        private readonly Queue<(AssemblyName, Assembly)> _referencedAssemblies = new Queue<(AssemblyName, Assembly)>();

        // Keeps track of the names we've already processed as referenced so we don't add them again
        private readonly HashSet<string> _referencedAssemblyNames = new HashSet<string>();

        // Key = assembly simple name, Value = assembly, direct
        private readonly Dictionary<string, (Assembly, bool)> _assembliesToLoad = new Dictionary<string, (Assembly, bool)>();

        // The full name of assemblies that have already been reflected
        private readonly HashSet<string> _reflectedAssemblyNames = new HashSet<string>();

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

        /// <summary>
        /// Adds a new assembly or pattern to those to be loaded.
        /// </summary>
        /// <param name="assembly">
        /// The assembly or pattern to load. If the string contains a "*" then it is considered a globbing pattern, otherwise it's considered an assembly.
        /// </param>
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

        /// <summary>
        /// Loads all specified assemblies and their references.
        /// </summary>
        internal void Load()
        {
            if (_loaded)
            {
                throw new InvalidOperationException("Assemblies have already been loaded");
            }
            _loaded = true;

            ProcessPatterns();

            ReflectAssemblies();
            ReflectReferencedAssemblies();
            LoadAssemblies();
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

        private void ReflectAssemblies()
        {
            foreach (string assemblyDeclaration in _assemblies)
            {
                string assembly = assemblyDeclaration.Trim().Trim('"');
                if (assembly.EndsWith(".dll") || assembly.EndsWith(".exe"))
                {
                    // If the path ends with .dll or .exe, attempt to load it as a path
                    ReflectAssemblyFromPath(assembly);
                }
                else
                {
                    // Attempt to load as a full name first and then by simple name if that fails
                    if (!ReflectionOnlyLoad(assembly, true))
                    {
                        ReflectAssemblyFromSimpleName(assembly);
                    }
                }
            }
        }

        private void ReflectAssemblyFromPath(string path)
        {
            FilePath filePath = new FilePath(path);

            // Attempt to load directly, and we're done if it's absolute regardless
            if (ReflectionOnlyLoadFrom(filePath.FullPath, true) || filePath.IsAbsolute)
            {
                return;
            }

            // Attempt to load from the entry assembly path
            if (ReflectionOnlyLoadFrom(_entryAssemblyDirectory.Path.CombineFile(filePath).FullPath, true))
            {
                return;
            }

            // Attempt to load from the build root
            ReflectionOnlyLoadFrom(_fileSystem.RootPath.CombineFile(filePath).FullPath, true);
        }

        private void ReflectAssemblyFromSimpleName(string name)
        {
            // Current algorithm is pretty hacky, take the assembly simple name and use the same version, etc. as a known framework library
            string fullName = typeof(object).Assembly.FullName;
            int firstSpace = fullName.IndexOf(",", StringComparison.Ordinal);  // a comma follows the short name
            if (firstSpace != -1)
            {
                fullName = name + fullName.Substring(firstSpace);
                Trace.Verbose($"Reflecting assembly {name} as full name {fullName}");
                ReflectionOnlyLoad(fullName, true);
            }
        }

        private bool ReflectionOnlyLoad(string assemblyString, bool direct)
        {
            // Check if we've already loaded it
            if (_reflectedAssemblyNames.Contains(assemblyString))
            {
                return true;
            }

            // Load the assembly for reflection
            using (Trace.WithIndent().Verbose($"Loading assembly {assemblyString} for reflection"))
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoad(assemblyString);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"{ex.GetType().Name} exception while reflecting assembly {assemblyString}: {ex.Message}");
                }
                if (assembly != null)
                {
                    _reflectedAssemblyNames.Add(assembly.FullName);
                    AddAssemblyToLoad(assembly, direct);
                    AddReferencedAssemblies(assembly);
                }
                return assembly != null;
            }
        }

        private bool ReflectionOnlyLoadFrom(string assemblyFile, bool direct)
        {
            // First load the name to verify the assembly and make sure it wasn't already loaded
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
                return false;
            }

            // Check if we've already loaded it
            if (_reflectedAssemblyNames.Contains(assemblyName.FullName))
            {
                Trace.Verbose($"Skipping assembly file {assemblyFile} because {assemblyName.FullName} has already been reflected");
                return true;
            }

            // Load the assembly for reflection
            using (Trace.WithIndent().Verbose($"Loading assembly {assemblyName.FullName} from file {assemblyFile} for reflection"))
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                }
                catch (Exception ex)
                {
                    Trace.Verbose($"{ex.GetType().Name} exception while reflecting assembly from file {assemblyFile}: {ex.Message}");
                }
                if (assembly != null)
                {
                    _reflectedAssemblyNames.Add(assembly.FullName);
                    AddAssemblyToLoad(assembly, direct);
                    AddReferencedAssemblies(assembly);
                }
                return assembly != null;
            }
        }

        private void AddAssemblyToLoad(Assembly assembly, bool direct)
        {
            AssemblyName assemblyName = assembly.GetName();
            (Assembly Assembly, bool Direct) existing;
            if (_assembliesToLoad.TryGetValue(assemblyName.Name, out existing))
            {
                if (existing.Assembly.GetName().Version > assemblyName.Version)
                {
                    // The existing version is higher, check if the direct flag needs to change
                    if (!existing.Direct && direct)
                    {
                        _assembliesToLoad[assemblyName.Name] = (existing.Assembly, true);
                    }
                    return;
                }

                // The new version is higher, resolve the direct flag
                direct = direct || existing.Direct;
            }
            _assembliesToLoad[assemblyName.Name] = (assembly, direct);
        }

        private void AddReferencedAssemblies(Assembly assembly)
        {
            foreach (AssemblyName referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (!_referencedAssemblyNames.Contains(referencedAssemblyName.FullName))
                {
                    Trace.Verbose($"Added referenced assembly {referencedAssemblyName}");
                    _referencedAssemblies.Enqueue((referencedAssemblyName, assembly));
                    _referencedAssemblyNames.Add(referencedAssemblyName.FullName);
                }
            }
        }

        // We need to load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private void ReflectReferencedAssemblies()
        {
            while (_referencedAssemblies.Count > 0)
            {
                (AssemblyName AssemblyName, Assembly ReferencingAssembly) reference = _referencedAssemblies.Dequeue();
                if (!_reflectedAssemblyNames.Contains(reference.AssemblyName.ToString()))
                {
                    using (Trace.WithIndent().Verbose($"Reflecting referenced assembly {reference.AssemblyName} (from {reference.ReferencingAssembly.GetName().Name})"))
                    {
                        // Try to load it by name
                        if (!ReflectionOnlyLoad(reference.AssemblyName.FullName, false))
                        {
                            // It wasn't loaded by name, so try polling at the referencing assembly location
                            string assemblyFile = Path.Combine(Path.GetDirectoryName(reference.ReferencingAssembly.Location), reference.AssemblyName.Name) + ".dll";
                            ReflectionOnlyLoadFrom(assemblyFile, false);
                        }
                    }
                }
            }
        }

        private void LoadAssemblies()
        {
            // Need to load assemblies in dependency order or else loading will fail with missing methods, etc.
            foreach ((Assembly Assembly, bool Direct) assemblyToLoad in _assembliesToLoad.OrderBy(x => x.Key).Select(x => x.Value))
            {
                using (Trace.WithIndent().Verbose($"Loading assembly {assemblyToLoad.Assembly.FullName} from {assemblyToLoad.Assembly.Location}"))
                {
                    Assembly assembly = null;

                    try
                    {
                        assembly = Assembly.LoadFrom(assemblyToLoad.Assembly.Location);
                    }
                    catch (Exception ex)
                    {
                        Trace.Verbose($"{ex.GetType().Name} exception while loading assembly from file {assemblyToLoad.Assembly.Location}: {ex.Message}");
                    }

                    if (assembly != null)
                    {
                        // Even though we probably already checked, be sure the assembly is in the collection
                        if (_assemblyCollection.Add(assembly))
                        {
                            Trace.Verbose($"Added assembly {assembly.FullName} from {assemblyToLoad.Assembly.Location} to the assembly collection");
                        }
                        else
                        {
                            Trace.Verbose($"Assembly {assembly.FullName} already added to assembly collection");
                        }

                        // Keep track of assemblies we directly asked for so we can scan them for the class catalog
                        if (assemblyToLoad.Direct)
                        {
                            DirectAssemblies.Add(assembly);
                        }
                    }
                }
            }
        }
    }
}