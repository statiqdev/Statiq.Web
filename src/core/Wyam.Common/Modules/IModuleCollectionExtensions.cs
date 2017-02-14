using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    public static class ModuleCollectionExtensions
    {
        public static void InsertAfter(this IModuleCollection moduleCollection, string name, params IModule[] modules) 
            => moduleCollection.Insert(moduleCollection.GuardIndexOf(name) + 1, modules);

        public static void InsertBefore(this IModuleCollection moduleCollection, string name, params IModule[] modules) 
            => moduleCollection.Insert(moduleCollection.GuardIndexOf(name), modules);

        public static void InsertBeforeFirst<T>(this IModuleCollection moduleCollection, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.InsertBeforeFirst<T>(_ => true, modules);

        public static void InsertBeforeFirst<T>(this IModuleCollection moduleCollection, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.Insert(moduleCollection.GuardIndexOfFirst(filter), modules);

        public static void InsertAfterFirst<T>(this IModuleCollection moduleCollection, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.InsertAfterFirst<T>(_ => true, modules);

        public static void InsertAfterFirst<T>(this IModuleCollection moduleCollection, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.Insert(moduleCollection.GuardIndexOfFirst(filter) + 1, modules);

        public static void InsertBeforeLast<T>(this IModuleCollection moduleCollection, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.InsertBeforeLast<T>(_ => true, modules);

        public static void InsertBeforeLast<T>(this IModuleCollection moduleCollection, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.Insert(moduleCollection.GuardIndexOfLast(filter), modules);

        public static void InsertAfterLast<T>(this IModuleCollection moduleCollection, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.InsertAfterLast<T>(_ => true, modules);

        public static void InsertAfterLast<T>(this IModuleCollection moduleCollection, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleCollection.Insert(moduleCollection.GuardIndexOfLast(filter) + 1, modules);

        public static void ReplaceFirst<T>(this IModuleCollection moduleCollection, IModule module)
            where T : class, IModule
            => moduleCollection.ReplaceFirst<T>(_ => true, module);

        public static void ReplaceFirst<T>(this IModuleCollection moduleCollection, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleCollection.Replace(moduleCollection.GuardIndexOfFirst(filter), module);

        public static void ReplaceLast<T>(this IModuleCollection moduleCollection, IModule module)
            where T : class, IModule
            => moduleCollection.ReplaceLast<T>(_ => true, module);

        public static void ReplaceLast<T>(this IModuleCollection moduleCollection, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleCollection.Replace(moduleCollection.GuardIndexOfLast(filter), module);

        public static void Replace(this IModuleCollection moduleCollection, string name, IModule module)
            => moduleCollection.Replace(moduleCollection.GuardIndexOf(name), module, name);

        public static void Replace(this IModuleCollection moduleCollection, int index, IModule module, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                NamedModule namedModule = moduleCollection[index] as NamedModule;
                if (namedModule != null)
                {
                    name = namedModule.Name;
                }
            }

            moduleCollection.RemoveAt(index);
            moduleCollection.Insert(index, name, module);
        }
        
        private static int GuardIndexOfLast<T>(this IModuleCollection moduleCollection, Predicate<T> filter)
            where T : class, IModule
        {
            for (int index = moduleCollection.Count - 1; index >= 0; index--)
            {
                IModule module = moduleCollection[index];
                T expectedModule = module as T;
                if (expectedModule == null)
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"Could not find module of type {typeof(T).FullName}");
        }

        private static int GuardIndexOfFirst<T>(this IModuleCollection moduleCollection, Predicate<T> filter) where T : class, IModule
        {
            for (int index = 0; index < moduleCollection.Count; index++)
            {
                IModule module = moduleCollection[index];
                T expectedModule = module as T;
                if (expectedModule == null)
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            throw new InvalidOperationException($"Could not find module of type {typeof(T).FullName}");
        }

        private static int GuardIndexOf(this IModuleCollection moduleCollection, string name)
        {
            int index = moduleCollection.IndexOf(name);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module with the name of {name}");
            }

            return index;
        }
    }
}
