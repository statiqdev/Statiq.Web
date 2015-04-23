using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Testing
{
    // Include this namespace to provide access to the internal prepare and execute methods for testing
    public static class TestingExtensions
    {        
        public static IEnumerable<IModuleContext> Prepare(this Module module, IModuleContext context)
        {
            return module.Prepare(context);
        }

        public static string Execute(this Module module, IModuleContext context, string content)
        {
            return module.Execute(context, content);
        }
    }
}
