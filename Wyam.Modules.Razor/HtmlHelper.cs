using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Modules.Razor.Microsoft.AspNet.Html.Abstractions;
using Wyam.Core;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor
{
    // Similar convention to ASP.NET MVC HtmlHelper (but totally different class, existing extensions won't work)
    public class HtmlHelper
    {
        private readonly ViewContext _viewContext;

        public HtmlHelper(ViewContext viewContext)
        {
            _viewContext = viewContext;
        }

        public IHtmlContent Raw(string value)
        {
            return new HtmlString(value);
        }

        public IHtmlContent Raw(object value)
        {
            return new HtmlString(value == null ? (string)null : value.ToString());
        }

        // Partial support from HtmlHelperPartialExtensions.cs
        // Core code is in HtmlHelper.cs

        public IHtmlContent Partial(string partialViewName)
        {
            using (var writer = new StringCollectionTextWriter(Encoding.UTF8))
            {
                RenderPartialCore(partialViewName, writer);
                return writer.Content;
            }
        }

        public void RenderPartial(string partialViewName)
        {
            RenderPartialCore(partialViewName, _viewContext.Writer);
        }

        private void RenderPartialCore(string partialViewName, TextWriter textWriter)
        {
            var viewEngineResult = _viewContext.ViewEngine.FindPartialView(_viewContext, partialViewName);
            if (!viewEngineResult.Success)
            {
                var locations = string.Empty;
                if (viewEngineResult.SearchedLocations != null)
                {
                    locations = Environment.NewLine +
                        string.Join(Environment.NewLine, viewEngineResult.SearchedLocations);
                }

                throw new InvalidOperationException(string.Format("Partial view {0} not found in {1}.", partialViewName, locations));
            }

            var view = viewEngineResult.View;
            using (view as IDisposable)
            {
                var viewContext = new ViewContext(_viewContext, view, _viewContext.ViewData, textWriter);
                AsyncHelper.RunSync(() => viewEngineResult.View.RenderAsync(viewContext));
            }
        }

        public ViewContext ViewContext
        {
            get { return _viewContext; }
        }
    }
}
