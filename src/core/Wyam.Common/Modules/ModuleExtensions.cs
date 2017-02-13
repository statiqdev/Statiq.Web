using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    public static class ModuleExtensions
    {
        public static IModule WithName(this IModule module, string name) => new NamedModule(name, module);
    }
}
