using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;

namespace Wyam.Modules.Razor
{
    // Similar convention to ASP.NET MVC HtmlHelper (but totally different class, existing extensions won't work)
    public class HtmlHelper
    {
        public HtmlString Raw(string value)
        {
            return new HtmlString(value);
        }

        public HtmlString Raw(object value)
        {
            return new HtmlString(value == null ? (string)null : value.ToString());
        }
    }
}
