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
        /// <param name="afterName">The name of the module at which to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static TModuleList InsertAfter<TModuleList>(this TModuleList moduleList, string afterName, params IModule[] modules)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.GuardIndexOf(afterName) + 1, modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the module with the specified name.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="afterName">The name of the module at which to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfter<TModuleList>(this TModuleList moduleList, string afterName, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.GuardIndexOf(afterName) + 1, name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the module with the specified name.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="beforeName">The name of the module at which to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBefore<TModuleList>(this TModuleList moduleList, string beforeName, params IModule[] modules)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.GuardIndexOf(beforeName), modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the module with the specified name.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="beforeName">The name of the module at which to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBefore<TModuleList>(this TModuleList moduleList, string beforeName, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.GuardIndexOf(beforeName), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeFirst<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<T>(this IModuleList moduleList, string name, IModule module)
            where T : class, IModule
            => moduleList.InsertBeforeFirst<T>(_ => true, name, module);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfFirst(filter), modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<T>(this IModuleList moduleList, Predicate<T> filter, string name, IModule module)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfFirst(filter), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterFirst<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<T>(this IModuleList moduleList, string name, IModule module)
            where T : class, IModule
            => moduleList.InsertAfterFirst<T>(_ => true, name, module);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfFirst(filter) + 1, modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<T>(this IModuleList moduleList, Predicate<T> filter, string name, IModule module)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfFirst(filter) + 1, name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertBeforeLast<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<T>(this IModuleList moduleList, string name, IModule module)
            where T : class, IModule
            => moduleList.InsertBeforeLast<T>(_ => true, name, module);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfLast(filter), modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<T>(this IModuleList moduleList, Predicate<T> filter, string name, IModule module)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfLast(filter), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<T>(this IModuleList moduleList, params IModule[] modules)
            where T : class, IModule
            => moduleList.InsertAfterLast<T>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<T>(this IModuleList moduleList, string name, IModule module)
            where T : class, IModule
            => moduleList.InsertAfterLast<T>(_ => true, name, module);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<T>(this IModuleList moduleList, Predicate<T> filter, params IModule[] modules)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfLast(filter) + 1, modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<T>(this IModuleList moduleList, Predicate<T> filter, string name, IModule module)
            where T : class, IModule
        {
            moduleList.Insert(moduleList.GuardIndexOfLast(filter) + 1, name, module);
            return moduleList;
        }

        /// <summary>
        /// Replaces the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceFirst<T>(this IModuleList moduleList, IModule module, string name = null)
            where T : class, IModule
            => moduleList.ReplaceFirst<T>(_ => true, module, name);

        /// <summary>
        /// Replaces the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceFirst<T>(this IModuleList moduleList, Predicate<T> filter, IModule module, string name = null)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfFirst(filter), module, name);

        /// <summary>
        /// Replaces the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceLast<T>(this IModuleList moduleList, IModule module, string name = null)
            where T : class, IModule
            => moduleList.ReplaceLast<T>(_ => true, module, name);

        /// <summary>
        /// Replaces the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceLast<T>(this IModuleList moduleList, Predicate<T> filter, IModule module, string name = null)
            where T : class, IModule
            => moduleList.Replace(moduleList.GuardIndexOfLast(filter), module, name);

        /// <summary>
        /// Replaces a module with the specified name. The replacement module will have the same name
        /// as the module being replaced unless an alternate name is specified.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="replaceName">The name of the module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static TModuleList Replace<TModuleList>(this TModuleList moduleList, string replaceName, IModule module, string name = null)
            where TModuleList : IModuleList
            => moduleList.Replace(moduleList.GuardIndexOf(replaceName), module, name ?? replaceName);

        /// <summary>
        /// Replaces a module at the specified index.
        /// </summary>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="index">The index of the module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">An optional name of the replacement module.</param>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <returns>The current instance.</returns>
        public static TModuleList Replace<TModuleList>(this TModuleList moduleList, int index, IModule module, string name = null)
            where TModuleList : IModuleList
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
            return moduleList;
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
