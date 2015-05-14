using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Razor
{
    // From https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNet.Mvc.Razor/ViewStartProvider.cs
    public class ViewStartProvider : IViewStartProvider
    {
        private readonly IRazorPageFactory _pageFactory;

        public ViewStartProvider(IRazorPageFactory pageFactory)
        {
            _pageFactory = pageFactory;
        }

        public IEnumerable<IRazorPage> GetViewStartPages(string path)
        {
            var viewStartLocations = ViewHierarchyUtility.GetViewStartLocations(path);
            var viewStarts = viewStartLocations.Select(_pageFactory.CreateInstance)
                                               .Where(p => p != null)
                                               .ToArray();

            // GetViewStartLocations return ViewStarts inside-out that is the _ViewStart closest to the page
            // is the first: e.g. [ /Views/Home/_ViewStart, /Views/_ViewStart, /_ViewStart ]
            // However they need to be executed outside in, so we'll reverse the sequence.
            Array.Reverse(viewStarts);

            return viewStarts;
        }
    }
}
