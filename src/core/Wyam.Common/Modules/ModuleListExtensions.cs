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
        /// Appends modules.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to append.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Append<TModuleList>(this TModuleList moduleList, params IModule[] modules)
            where TModuleList : IModuleList
        {
            moduleList.Add(modules);
            return moduleList;
        }

        /// <summary>
        /// Appends a named module.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to append.</param>
        /// <param name="module">The module to append.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Append<TModuleList>(this TModuleList moduleList, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Add(name, module);
            return moduleList;
        }

        /// <summary>
        /// Prepends modules.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to prepend.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Prepend<TModuleList>(this TModuleList moduleList, params IModule[] modules)
            where TModuleList : IModuleList
        {
            moduleList.Insert(0, modules);
            return moduleList;
        }

        /// <summary>
        /// Prepends a named module.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to prepend.</param>
        /// <param name="module">The module to prepend.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Prepend<TModuleList>(this TModuleList moduleList, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Insert(0, name, module);
            return moduleList;
        }

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
            moduleList.Insert(moduleList.IndexOfOrThrow(afterName) + 1, modules);
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
        public static TModuleList InsertAfter<TModuleList>(this TModuleList moduleList, string afterName, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.IndexOfOrThrow(afterName) + 1, name, module);
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
        public static TModuleList InsertBefore<TModuleList>(this TModuleList moduleList, string beforeName, params IModule[] modules)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.IndexOfOrThrow(beforeName), modules);
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
        public static TModuleList InsertBefore<TModuleList>(this TModuleList moduleList, string beforeName, string name, IModule module)
            where TModuleList : IModuleList
        {
            moduleList.Insert(moduleList.IndexOfOrThrow(beforeName), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<TModule>(this IModuleList moduleList, params IModule[] modules)
            where TModule : class, IModule
            => moduleList.InsertBeforeFirst<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<TModule>(this IModuleList moduleList, string name, IModule module)
            where TModule : class, IModule
            => moduleList.InsertBeforeFirst<TModule>(_ => true, name, module);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfFirstOrThrow(filter), modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter, string name, IModule module)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfFirstOrThrow(filter), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<TModule>(this IModuleList moduleList, params IModule[] modules)
            where TModule : class, IModule
            => moduleList.InsertAfterFirst<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<TModule>(this IModuleList moduleList, string name, IModule module)
            where TModule : class, IModule
            => moduleList.InsertAfterFirst<TModule>(_ => true, name, module);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfFirstOrThrow(filter) + 1, modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter, string name, IModule module)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfFirstOrThrow(filter) + 1, name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<TModule>(this IModuleList moduleList, params IModule[] modules)
            where TModule : class, IModule
            => moduleList.InsertBeforeLast<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<TModule>(this IModuleList moduleList, string name, IModule module)
            where TModule : class, IModule
            => moduleList.InsertBeforeLast<TModule>(_ => true, name, module);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfLastOrThrow(filter), modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertBeforeLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter, string name, IModule module)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfLastOrThrow(filter), name, module);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<TModule>(this IModuleList moduleList, params IModule[] modules)
            where TModule : class, IModule
            => moduleList.InsertAfterLast<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<TModule>(this IModuleList moduleList, string name, IModule module)
            where TModule : class, IModule
            => moduleList.InsertAfterLast<TModule>(_ => true, name, module);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfLastOrThrow(filter) + 1, modules);
            return moduleList;
        }

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="name">The name of the module to insert.</param>
        /// <param name="module">The module to insert.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList InsertAfterLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter, string name, IModule module)
            where TModule : class, IModule
        {
            moduleList.Insert(moduleList.IndexOfLastOrThrow(filter) + 1, name, module);
            return moduleList;
        }

        /// <summary>
        /// Replaces the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceFirst<TModule>(this IModuleList moduleList, IModule module, string name = null)
            where TModule : class, IModule
            => moduleList.ReplaceFirst<TModule>(_ => true, module, name);

        /// <summary>
        /// Replaces the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter, IModule module, string name = null)
            where TModule : class, IModule
            => moduleList.Replace(moduleList.IndexOfFirstOrThrow(filter), module, name);

        /// <summary>
        /// Replaces the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceLast<TModule>(this IModuleList moduleList, IModule module, string name = null)
            where TModule : class, IModule
            => moduleList.ReplaceLast<TModule>(_ => true, module, name);

        /// <summary>
        /// Replaces the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <param name="name">The name of the replacement module.</param>
        /// <returns>The current instance.</returns>
        public static IModuleList ReplaceLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter, IModule module, string name = null)
            where TModule : class, IModule
            => moduleList.Replace(moduleList.IndexOfLastOrThrow(filter), module, name);

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
            => moduleList.Replace(moduleList.IndexOfOrThrow(replaceName), module, name ?? replaceName);

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

        /// <summary>
        /// Modifies an inner module with the specified name.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="modifyName">The name of the inner module to modify.</param>
        /// <param name="action">The action to apply to the inner module.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Modify<TModuleList>(this TModuleList moduleList, string modifyName, Action<IModule> action)
            where TModuleList : IModuleList
            => moduleList.Modify(moduleList.IndexOfOrThrow(modifyName), action);

        /// <summary>
        /// Modifies an inner module list with the specified index.
        /// </summary>
        /// <typeparam name="TModuleList">The type of the module list.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/>.</param>
        /// <param name="index">The index of the inner module to modify.</param>
        /// <param name="action">The action to apply to the inner module.</param>
        /// <returns>The current instance.</returns>
        public static TModuleList Modify<TModuleList>(this TModuleList moduleList, int index, Action<IModule> action)
            where TModuleList : IModuleList
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            action(moduleList[index]);
            return moduleList;
        }

        /// <summary>
        /// Gets the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <returns>The first module of the specified type or null if a module of the specified type could not be found.</returns>
        public static TModule GetFirst<TModule>(this IModuleList moduleList)
            where TModule : class, IModule
            => moduleList.GetFirst<TModule>(_ => true);

        /// <summary>
        /// Gets the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The first module of the specified type or null if a module of the specified type could not be found.</returns>
        public static TModule GetFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = moduleList.IndexOfFirst(filter);
            return index == -1 ? default(TModule) : (TModule)moduleList[index];
        }

        /// <summary>
        /// Gets the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <returns>The last module of the specified type or null if a module of the specified type could not be found.</returns>
        public static TModule GetLast<TModule>(this IModuleList moduleList)
            where TModule : class, IModule
            => moduleList.GetLast<TModule>(_ => true);

        /// <summary>
        /// Gets the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The last module of the specified type or null if a module of the specified type could not be found.</returns>
        public static TModule GetLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = moduleList.IndexOfLast(filter);
            return index == -1 ? default(TModule) : (TModule)moduleList[index];
        }

        /// <summary>
        /// Gets the index of the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <returns>The index of the first module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public static int IndexOfFirst<TModule>(this IModuleList moduleList)
            where TModule : class, IModule
            => moduleList.IndexOfFirst<TModule>(_ => true);

        /// <summary>
        /// Gets the index of the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The index of the first module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public static int IndexOfFirst<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            for (int index = 0; index < moduleList.Count; index++)
            {
                IModule module = moduleList[index];

                if (!(module is TModule expectedModule))
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <returns>The index of the last module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public static int IndexOfLast<TModule>(this IModuleList moduleList)
            where TModule : class, IModule
            => moduleList.IndexOfLast<TModule>(_ => true);

        /// <summary>
        /// Gets the index of the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="moduleList">The <see cref="ModuleList"/> to search.</param>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The index of the last module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public static int IndexOfLast<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            for (int index = moduleList.Count - 1; index >= 0; index--)
            {
                IModule module = moduleList[index];

                if (!(module is TModule expectedModule))
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int IndexOfFirstOrThrow<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = moduleList.IndexOfFirst(filter);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module of type {typeof(TModule).FullName}");
            }
            return index;
        }

        private static int IndexOfLastOrThrow<TModule>(this IModuleList moduleList, Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = moduleList.IndexOfLast(filter);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module of type {typeof(TModule).FullName}");
            }
            return index;
        }

        private static int IndexOfOrThrow(this IModuleList moduleList, string name)
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
