using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/RazorPageResult.cs
    /// <summary>
    /// Represents the results of locating a <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorPageResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RazorPageResult"/> for a successful discovery.
        /// </summary>
        /// <param name="name">The name of the page that was located.</param>
        /// <param name="page">The located <see cref="IRazorPage"/>.</param>
        public RazorPageResult(string name, IRazorPage page)
        {
            Name = name;
            Page = page;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RazorPageResult"/> for an unsuccessful discovery.
        /// </summary>
        /// <param name="name">The name of the page that was located.</param>
        /// <param name="page">The locations that were searched.</param>
        public RazorPageResult(string name, IEnumerable<string> searchedLocations)
        {
            Name = name;
            SearchedLocations = searchedLocations;
        }

        /// <summary>
        /// Gets the name of the page being located.
        /// </summary>
        /// <remarks>This property maps to the <c>name</c> parameter of
        /// <see cref="IRazorViewEngine.FindPage(ActionContext, string)"/>.</remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the <see cref="IRazorPage"/> if found.
        /// </summary>
        /// <remarks>This property is <c>null</c> if the page was not found.</remarks>
        public IRazorPage Page { get; private set; }

        /// <summary>
        /// Gets the locations that were searched when <see cref="Page"/> could not be located.
        /// </summary>
        /// <remarks>This property is <c>null</c> if the page was found.</remarks>
        public IEnumerable<string> SearchedLocations { get; private set; }
    }
}
