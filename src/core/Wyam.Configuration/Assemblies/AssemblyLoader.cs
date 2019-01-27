using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Reflection.Metadata;
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
        private readonly ConcurrentQueue<(AssemblyName, Assembly)> _referencedAssemblies = new ConcurrentQueue<(AssemblyName, Assembly)>();

        // Keeps track of the names we've already processed as referenced so we don't add them again
        private readonly ConcurrentHashSet<string> _referencedAssemblyNames = new ConcurrentHashSet<string>();

        // Key = assembly simple name, Value = assembly, direct
        private readonly ConcurrentDictionary<string, (Assembly, bool)> _assembliesToLoad = new ConcurrentDictionary<string, (Assembly, bool)>();

        // The full name of assemblies that have already been reflected
        private readonly ConcurrentHashSet<string> _reflectedAssemblyNames = new ConcurrentHashSet<string>();

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

            AddLoadedAssemblies();
            ProcessPatterns();
            ReflectAssemblies();
            ReflectReferencedAssemblies();
            LoadAssemblies();
        }

        private void AddLoadedAssemblies()
        {
            using (Trace.WithIndent().Verbose("Adding already loaded assemblies to the collection"))
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AddToAssemblyCollection(assembly, true);
                }
            }
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

            Trace.Verbose($"{_assemblies.Count} assemblies identified for loading");
        }

        private void ReflectAssemblies()
        {
            using (Trace.WithIndent().Verbose("Reflecting assemblies to find references"))
            {
                Parallel.ForEach(_assemblies, assemblyDeclaration =>
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
                });
            }
        }

        // We need to load all referenced assemblies so they can be provided in the execution context to any modules doing dynamic compilation (I.e., Razor)
        private void ReflectReferencedAssemblies()
        {
            using (Trace.WithIndent().Verbose("Reflecting referenced assemblies to find nested references"))
            {
                // Get the referenced assemblies in batches and process each batch in parallel
                List<(AssemblyName AssemblyName, Assembly ReferencingAssembly)> references = new List<(AssemblyName, Assembly)>();
                while (!_referencedAssemblies.IsEmpty)
                {
                    // Create the batch
                    references.Clear();
                    (AssemblyName AssemblyName, Assembly ReferencingAssembly) queuedReference;
                    while (_referencedAssemblies.TryDequeue(out queuedReference))
                    {
                        references.Add(queuedReference);
                    }

                    // Process the batch
                    Trace.Verbose($"Reflecting batch of {references.Count} reference assemblies");
                    Parallel.ForEach(references, reference =>
                    {
                        if (!_reflectedAssemblyNames.Contains(reference.AssemblyName.ToString()))
                        {
                            Trace.Verbose($"Reflecting referenced assembly {reference.AssemblyName} (from {reference.ReferencingAssembly.GetName().Name})");

                            // Try polling at the referencing assembly location
                            string assemblyFile = Path.Combine(Path.GetDirectoryName(reference.ReferencingAssembly.Location), reference.AssemblyName.Name) + ".dll";
                            if (!ReflectionOnlyLoadFrom(assemblyFile, false))
                            {
                                // Couldn't find the file so try loading by name
                                ReflectionOnlyLoad(reference.AssemblyName.FullName, false);
                            }
                        }
                    });
                }
            }
        }

        private void LoadAssemblies()
        {
            using (Trace.WithIndent().Verbose($"Loading {_assembliesToLoad.Count} assemblies (including references)"))
            {
                // Need to load assemblies in dependency order or else loading will fail with missing methods, etc.
                foreach ((Assembly Assembly, bool Direct) assemblyToLoad in _assembliesToLoad.Values.OrderBy(x => x.Item1, new AssemblyDependencyComparer()).ToArray())
                {
                    using (Trace.WithIndent().Verbose($"Loading assembly {assemblyToLoad.Assembly.FullName} from {assemblyToLoad.Assembly.Location}"))
                    {
                        Assembly assembly = null;

                        try
                        {
                            assembly = Assembly.Load(assemblyToLoad.Assembly.FullName);
                        }
                        catch (Exception ex)
                        {
                            Trace.Verbose($"{ex.GetType().Name} exception while loading assembly by full name: {ex.Message}");
                            assembly = null;
                        }

                        if (assembly != null && assembly.FullName != assemblyToLoad.Assembly.FullName)
                        {
                            Trace.Verbose($"Assembly redirected to {assembly.FullName}");
                        }

                        if (assembly == null)
                        {
                            try
                            {
                                // This should act the same as calling .LoadFrom() but will load the assembly into the default context
                                // See https://github.com/Microsoft/MSBuildLocator/issues/8#issue-285040083
                                AssemblyName name = AssemblyName.GetAssemblyName(assemblyToLoad.Assembly.Location);
                                assembly = Assembly.Load(name);
                            }
                            catch (Exception ex)
                            {
                                Trace.Verbose($"{ex.GetType().Name} exception while loading assembly from file: {ex.Message}");
                            }
                        }

                        if (assembly != null)
                        {
                            AddToAssemblyCollection(assembly, assemblyToLoad.Direct);
                        }
                    }
                }

                // Now that we're done, all required assemblies should be loaded and we can report cache misses
                _assemblyResolver.ReportCacheMisses = true;
            }
        }

        // ** Helpers

        /// <summary>
        /// Returns assemblies in dependency order.
        /// </summary>
        private class AssemblyDependencyComparer : IComparer<Assembly>
        {
            public int Compare(Assembly a, Assembly b)
            {
                string aName = a.GetName().Name;
                string bName = b.GetName().Name;
                if (a.GetReferencedAssemblies().Any(x => x.Name == bName))
                {
                    return 1;
                }
                if (b.GetReferencedAssemblies().Any(x => x.Name == aName))
                {
                    return -1;
                }
                return a.GetReferencedAssemblies().Length - b.GetReferencedAssemblies().Length;
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
            Trace.Verbose($"Loading assembly {assemblyString} by name for reflection");

            // Check if we've already loaded it
            if (_reflectedAssemblyNames.Contains(assemblyString) || _assemblyCollection.ContainsFullName(assemblyString))
            {
                Trace.Verbose($"Skipping assembly {assemblyString} because it has already been loaded");
                return true;
            }

            // Load the assembly for reflection
            Assembly assembly = null;
            try
            {
                // .NET Core doesn't support ReflectionOnlyLoad()
                // assembly = Assembly.ReflectionOnlyLoad(assemblyString);
                assembly = Assembly.Load(assemblyString);
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

        private bool ReflectionOnlyLoadFrom(string assemblyFile, bool direct)
        {
            Trace.Verbose($"Loading assembly file {assemblyFile} for reflection");

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
            if (_reflectedAssemblyNames.Contains(assemblyName.FullName) || _assemblyCollection.ContainsFullName(assemblyName.FullName))
            {
                Trace.Verbose($"Skipping assembly file {assemblyFile} because {assemblyName.FullName} has already been loaded");
                return true;
            }

            // Load the assembly for reflection
            Assembly assembly = null;
            try
            {
                // .NET Core doesn't support ReflectionOnlyLoad() - not going to worry too much about this since it's going away soon
                // assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                assembly = Assembly.LoadFrom(assemblyFile);
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
                if (!_referencedAssemblyNames.Contains(referencedAssemblyName.FullName) && !_assemblyCollection.ContainsFullName(referencedAssemblyName.FullName))
                {
                    Trace.Verbose($"Added referenced assembly {referencedAssemblyName}");
                    _referencedAssemblies.Enqueue((referencedAssemblyName, assembly));
                    _referencedAssemblyNames.Add(referencedAssemblyName.FullName);
                }
            }
        }

        private void AddToAssemblyCollection(Assembly assembly, bool direct)
        {
            if (_assemblyCollection.Add(assembly))
            {
                string framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown target";
                Trace.Verbose($"Added assembly {assembly.FullName} from {(assembly.IsDynamic ? "dynamic" : assembly.Location)} ({framework}) to the assembly collection");
            }
            else
            {
                Trace.Verbose($"Assembly {assembly.FullName} already added to assembly collection");
            }

            // Keep track of assemblies we directly asked for so we can scan them for the class catalog
            if (direct)
            {
                DirectAssemblies.Add(assembly);
            }
        }
    }
}