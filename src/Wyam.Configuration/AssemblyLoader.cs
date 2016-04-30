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
    internal class AssemblyLoader
    {
        // TODO: Stop loading all referenced assemblies (I.e., we don't need to load all the nuget assemblies)
        // TODO: Store the added globbing patterns in a concurrent safe hash set
        private readonly List<Tuple<DirectoryPath, SearchOption>> _directories = new List<Tuple<DirectoryPath, SearchOption>>();
        private readonly List<FilePath> _byFile = new List<FilePath>();
        private readonly List<string> _byName = new List<string>();
        private readonly HashSet<Type> _moduleTypes = new HashSet<Type>();

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
        public void LoadAssemblies(PackageInstaller packages, IReadOnlyFileSystem fileSystem, IAssemblyCollection assemblies, INamespacesCollection namespaces)
        {
            // Add all module namespaces from Wyam.Core
            namespaces.AddRange(typeof(Engine).Assembly.GetTypes()
                .Where(x => typeof(IModule).IsAssignableFrom(x))
                .Select(x => x.Namespace));

            // Also include all Wyam.Common namespaces
            namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                .Select(x => x.Namespace));

            // Get path to all assemblies (except those specified by name)
            List<FilePath> assemblyPaths = new List<FilePath>();
            //assemblyPaths.AddRange(packages.GetCompatibleAssemblyPaths()); // TODO: Once NuGet stuff is complete
            string entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
            if (entryAssemblyLocation != null)
            {
                assemblyPaths.AddRange(Directory
                    .GetFiles(new FilePath(entryAssemblyLocation).Directory.FullPath, "*.dll", SearchOption.AllDirectories)
                    .Select(x => new FilePath(x)));
            }
            assemblyPaths.AddRange(_directories
                .Select(x => new Tuple<DirectoryPath, SearchOption>(fileSystem.RootPath.Combine(x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1.FullPath))
                .SelectMany(x => Directory.GetFiles(x.Item1.FullPath, "*.dll", x.Item2).Select(y => new FilePath(y))));
            assemblyPaths.AddRange(_byFile
                .Select(x => new Tuple<FilePath, FilePath>(x, fileSystem.RootPath.CombineFile(x)))
                .Select(x => File.Exists(x.Item2.FullPath) ? x.Item2 : x.Item1));

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";", 
                    new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => x.Directory.FullPath))
                    .Distinct());

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
                    if (!assemblies.Add(assembly))
                    {
                        Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assemblies, assembly.GetReferencedAssemblies());
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
                    if (!assemblies.Add(assembly))
                    {
                        Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assemblies, assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }

            // Scan for required types
            FindModuleTypes(assemblies, namespaces);
        }

        private void LoadReferencedAssemblies(IAssemblyCollection assemblies, IEnumerable<AssemblyName> assemblyNames)
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
    }
}