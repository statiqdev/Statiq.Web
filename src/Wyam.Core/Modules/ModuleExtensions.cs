using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.Core.Modules
{
    public static class ModuleExtensions
    {
        internal static ConcurrentHashSet<IModule> AsNewDocumentModules { get; } = new ConcurrentHashSet<IModule>();

        public static TModule AsNewDocuments<TModule>(this TModule module) where TModule : IModule, IAsNewDocuments
        {
            AsNewDocumentModules.Add(module);
            return module;
        }
    }
}
