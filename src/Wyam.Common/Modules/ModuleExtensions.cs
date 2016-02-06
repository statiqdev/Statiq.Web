using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    public static class ModuleExtensions
    {
        public static TModule AsNewDocuments<TModule>(this TModule module) where TModule : IModule
        {
            return module;
        }
    }
}
