using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// Extensions for use with <see cref="ModuleList"/>.
    /// </summary>
    public static class ModuleListExtensions
    {
        /// <summary>
        /// Inserts modules after the module with the specified name.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module at which to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertAfter(this IModuleList moduleList, string name, params IModule[] modules)
            => moduleList.Insert(moduleList.GuardIndexOf(name) + 1, modules);

        /// <summary>
        /// Inserts modules before the module with the specified name.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module at which to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertBefore(this IModuleList moduleList, string name, params IModule[] modules)
            => moduleList.Insert(moduleList.GuardIndexOf(name), modules);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertBeforeFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeFirst<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertBeforeFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfFirst(filter), modules);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertAfterFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterFirst<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertAfterFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfFirst(filter) + 1, modules);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertBeforeLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeLast<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertBeforeLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfLast(filter), modules);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertAfterLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterLast<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public static void InsertAfterLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
            => moduleList.Insert(moduleList.GuardIndexOfLast(filter) + 1, modules);

        /// <summary>
        /// Replaces the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        public static void ReplaceFirst<T>(this IModuleList moduleList, IModule module)
            where T : class, IModule
            => moduleList.ReplaceFirst<T>(_ => true, module);

        /// <summary>
        /// Replaces the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        public static void ReplaceFirst<T>(this IModuleList moduleList, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfFirst(filter), module);

        /// <summary>
        /// Replaces the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        public static void ReplaceLast<T>(this IModuleList moduleList, IModule module)
            where T : class, IModule
            => moduleList.ReplaceLast<T>(_ => true, module);

        /// <summary>
        /// Replaces the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        public static void ReplaceLast<T>(this IModuleList moduleList, Predicate<T> filter, IModule module)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfLast(filter), module);

        /// <summary>
        /// Replaces a module with the specified name. The replacement module will have the same name
        /// as the module being replaced.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to replace.</param>
        /// <param name="module">The replacement module.</param>
        public static void Replace(this IModuleList moduleList, string name, IModule module)
            => moduleList.Replace(moduleList.GuardIndexOf(name), module, name);

        /// <summary>
        /// Replaces a module at the specified index.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="index">The index of the module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">An optional name of the replacement module.</param>
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
