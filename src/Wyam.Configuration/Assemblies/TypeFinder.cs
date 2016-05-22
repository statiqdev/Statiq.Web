using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Configuration.Preprocessing;
using Wyam.Core.Configuration;

namespace Wyam.Configuration.Assemblies
{
    /// <summary>
    /// Responsible for iterating over a set of assemblies
    /// looking for implementations of predefined interfaces.
    /// </summary>
    public class TypeFinder
    {
        private readonly INamespacesCollection _namespaces;

        private static readonly Type[] Types =
        {
            typeof(IModule)
        };

        private readonly Dictionary<Type, ConcurrentHashSet<Type>> _implementations
            = new Dictionary<Type, ConcurrentHashSet<Type>>();

        public TypeFinder(INamespacesCollection namespaces)
        {
            _namespaces = namespaces;

            foreach (Type type in Types)
            {
                _implementations.Add(type, new ConcurrentHashSet<Type>());
            }
        }

        public IEnumerable<Type> GetImplementations<TType>()
        {
            ConcurrentHashSet<Type> implementations;
            return _implementations.TryGetValue(typeof(TType), out implementations)
                ? (IEnumerable<Type>) implementations
                : Array.Empty<Type>();
        }

        /// <summary>
        /// Loads the types.
        /// </summary>
        /// <returns>Namespaces for types that require them (I.e., modules)</returns>
        public void FindTypes(HashSet<Assembly> assemblies)
        {
            Parallel.ForEach(assemblies, assembly =>
            {
                Trace.Verbose($"Searching for types in assembly {assembly.FullName}...");

                // Get all loadable types in the assembly
                Type[] loadableTypes = GetLoadableTypes(assembly);
                if (loadableTypes != null && loadableTypes.Length > 0)
                {
                    Parallel.ForEach(Types, type =>
                    {
                        ConcurrentHashSet<Type> typeSet = _implementations[type];

                        // Look for implementations
                        foreach (Type implementation in loadableTypes
                            .Where(x => type.IsAssignableFrom(x) && x.IsPublic && !x.IsAbstract && x.IsClass))
                        {
                            Trace.Verbose($"Found {type.Name} {implementation.Name} in assembly {assembly.FullName}");
                            typeSet.Add(implementation);
                            _namespaces.Add(implementation.Namespace);
                        }
                    });
                }

                Trace.Verbose($"Finished searching for types in assembly {assembly.FullName}");
            });
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
                    Trace.Verbose($"Loader Exception for assembly {assembly.FullName}: {loaderException.Message}");
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
