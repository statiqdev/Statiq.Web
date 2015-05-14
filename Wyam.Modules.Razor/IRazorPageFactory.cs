using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/IRazorPageFactory.cs
    public interface IRazorPageFactory
    {
        /// <summary>
        /// Creates a <see cref="IRazorPage"/> for the specified path.
        /// </summary>
        /// <param name="relativePath">The path to locate the page.</param>
        /// <returns>The IRazorPage instance if it exists, null otherwise.</returns>
        IRazorPage CreateInstance(string relativePath);
    }
}
