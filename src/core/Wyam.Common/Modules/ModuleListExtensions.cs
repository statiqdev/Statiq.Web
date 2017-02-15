using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    public static class ModuleListExtensions
    {
        public static void InsertAfter(this IModuleList moduleList, string name, params IModule[] modules) 
            => moduleList.Insert(moduleList.GuardIndexOf(name) + 1, modules);

        public static void InsertBefore(this IModuleList moduleList, string name, params IModule[] modules) 
            => moduleList.Insert(moduleList.GuardIndexOf(name), modules);

        public static void InsertBeforeFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeFirst<T>(_ => true, modules);

        public static void InsertBeforeFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfFirst(filter), modules);

        public static void InsertAfterFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterFirst<T>(_ => true, modules);

        public static void InsertAfterFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfFirst(filter) + 1, modules);

        public static void InsertBeforeLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeLast<T>(_ => true, modules);

        public static void InsertBeforeLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfLast(filter), modules);

        public static void InsertAfterLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterLast<T>(_ => true, modules);

        public static void InsertAfterLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfLast(filter) + 1, modules);

        public static void ReplaceFirst<T>(this IModuleList moduleList, IModule module)
            where T : class, IModule
            => moduleList.ReplaceFirst<T>(_ => true, module);

        public static void ReplaceFirst<T>(this IModuleList moduleList, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfFirst(filter), module);

        public static void ReplaceLast<T>(this IModuleList moduleList, IModule module)
            where T : class, IModule
            => moduleList.ReplaceLast<T>(_ => true, module);

        public static void ReplaceLast<T>(this IModuleList moduleList, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfLast(filter), module);

        public static void Replace(this IModuleList moduleList, string name, IModule module)
            => moduleList.Replace(moduleList.GuardIndexOf(name), module, name);

        public static void Replace(this IModuleList moduleList, int index, IModule module, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                NamedModule namedModule = moduleList[index] as NamedModule;
                if (namedModule != null)
                {
                    name = namedModule.Name;
                }
            }

            moduleList.RemoveAt(index);
            moduleList.Insert(index, name, module);
        }
        
        private static int GuardIndexOfLast<T>(this IModuleList moduleList, Predicate<T> filter)
            where T : class, IModule
        {
            for (int index = moduleList.Count - 1; index >= 0; index--)
            {
                IModule module = moduleList[index];
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

        private static int GuardIndexOfFirst<T>(this IModuleList moduleList, Predicate<T> filter) where T : class, IModule
        {
            for (int index = 0; index < moduleList.Count; index++)
            {
                IModule module = moduleList[index];
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

        private static int GuardIndexOf(this IModuleList moduleList, string name)
        {
            int index = moduleList.IndexOf(name);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module with the name of {name}");
            }

            return index;
        }
    }
}
