using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/IRazorViewEngine.cs
    public interface IRazorViewEngine
    {
        RazorPageResult FindPage(ViewContext context, string page);
    }
}
