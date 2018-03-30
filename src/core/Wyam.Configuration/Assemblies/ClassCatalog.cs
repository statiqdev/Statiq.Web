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
    public class ClassCatalog
    {
        private readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Gets all classes of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of classes to get.</typeparam>
        /// <returns>All classes of type <see cref="T"/>.</returns>
        public IEnumerable<Type> GetClasses<T>() => _types.Values.Where(x => typeof(T).IsAssignableFrom(x));

        /// <summary>
        /// Gets instances for all classes of a specified type..
        /// </summary>
        /// <typeparam name="T">The type of instances to get.</typeparam>
        /// <returns>Instances for all classes of type <see cref="T"/>.</returns>
        public IEnumerable<T> GetInstances<T>() => GetClasses<T>().Select(x => (T)Activator.CreateInstance(x));

        /// <summary>
        /// Gets an instance for a class of a specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the instance to get.</typeparam>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> ignore the case of the type name.</param>
        /// <returns>
        /// An instance of the first class that matches the specified type and name.
        /// </returns>
        public T GetInstance<T>(string typeName, bool ignoreCase = false)
        {
            Type type = GetClasses<T>().FirstOrDefault(x => x.Name.Equals(typeName,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            return type == null ? default(T) : (T)Activator.CreateInstance(type);
        }

        public void CatalogTypes(IEnumerable<Assembly> assemblies)
        {
            Parallel.ForEach(assemblies, assembly =>
            {
                Trace.Verbose($"Cataloging types in assembly {assembly.FullName}");
                foreach (Type type in GetLoadableTypes(assembly).Where(x => x.IsPublic && !x.IsAbstract && x.IsClass))
                {
                    _types.TryAdd(type.FullName, type);
                }
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
                    Trace.Verbose($"ReflectionTypeLoadException for assembly {assembly.FullName}: {loaderException.Message}");
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
