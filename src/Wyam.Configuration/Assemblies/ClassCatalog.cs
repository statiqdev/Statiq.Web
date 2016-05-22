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
        private readonly ConcurrentDictionary<Assembly, List<Type>> _classes
            = new ConcurrentDictionary<Assembly, List<Type>>();

        public IEnumerable<Type> GetClasses<T>() =>
            _classes.Values.SelectMany(x => x).Where(x => typeof(T).IsAssignableFrom(x));

        public IEnumerable<T> GetInstances<T>() =>
            GetClasses<T>().Select(x => (T) Activator.CreateInstance(x));
        
        public void CatalogTypes(HashSet<Assembly> assemblies)
        {
            Parallel.ForEach(assemblies, assembly =>
            {
                Trace.Verbose($"Cataloging types in assembly {assembly.FullName}");
                _classes.TryAdd(assembly,
                    GetLoadableTypes(assembly).Where(x => x.IsPublic && !x.IsAbstract && x.IsClass).ToList());
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
