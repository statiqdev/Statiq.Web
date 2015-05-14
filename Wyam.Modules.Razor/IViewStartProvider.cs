using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/IViewStartProvider.cs
    /// <summary>
    /// Defines methods for locating ViewStart pages that are applicable to a page.
    /// </summary>
    public interface IViewStartProvider
    {
        /// <summary>
        /// Given a view path, returns a sequence of ViewStart instances
        /// that are applicable to the specified view.
        /// </summary>
        /// <param name="path">The path of the page to locate ViewStart files for.</param>
        /// <returns>A sequence of <see cref="IRazorPage"/> that represent ViewStart.</returns>
        IEnumerable<IRazorPage> GetViewStartPages(string path);
    }
}
