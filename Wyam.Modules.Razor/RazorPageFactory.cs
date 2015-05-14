using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // This is where we start to diverge from the MVC Razor implementation
    // It has all kinds of fancy compilation support, we're just going to use Roslyn for this
    // See https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/VirtualPathRazorPageFactory.cs
    // ...which relies heavily on https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/Compilation/RazorCompilationService.cs
    // .. which then relies on https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/Compilation/RoslynCompilationService.cs
    public class RazorPageFactory : IRazorPageFactory
    {
        public IRazorPage CreateInstance(string relativePath)
        {
            throw new NotImplementedException();
        }
    }
}
