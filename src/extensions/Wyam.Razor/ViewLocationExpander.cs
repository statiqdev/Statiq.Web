using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Wyam.Razor
{
    internal class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return new[]
            {
                "/{0}",
                "/Shared/{0}",
                "/Views/{0}",
                "/Views/Shared/{0}",
                "/{0}.cshtml",
                "/Shared/{0}.cshtml",
                "/Views/{0}.cshtml",
                "/Views/Shared/{0}.cshtml"
            };
        }
    }
}