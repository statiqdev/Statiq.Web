using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Razor.Microsoft.AspNet.Html.Abstractions;
using Wyam.Razor.Microsoft.AspNet.Mvc;
using Wyam.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Razor.Microsoft.Framework.Internal;

namespace Wyam.Razor
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

        public async Task<IHtmlContent> PartialAsync(
            [NotNull] string partialViewName)
        {
            using (var writer = new StringCollectionTextWriter(Encoding.UTF8))
            {
                await RenderPartialCoreAsync(partialViewName, writer);
                return writer.Content;
            }
        }

        public Task RenderPartialAsync([NotNull] string partialViewName)
        {
            return RenderPartialCoreAsync(partialViewName, _viewContext.Writer);
        }

        protected virtual async Task RenderPartialCoreAsync([NotNull] string partialViewName,
                                                            TextWriter writer)
        {
            var viewEngineResult = _viewContext.ViewEngine.FindPartialView(ViewContext, partialViewName);
            if (!viewEngineResult.Success)
            {
                var locations = string.Empty;
                if (viewEngineResult.SearchedLocations != null)
                {
                    locations = Environment.NewLine +
                        string.Join(Environment.NewLine, viewEngineResult.SearchedLocations);
                }

                throw new InvalidOperationException($"Partial view {partialViewName} not found in {locations}.");
            }

            var view = viewEngineResult.View;
            using (view as IDisposable)
            {
                var viewContext = new ViewContext(_viewContext, view, _viewContext.ViewData, writer);
                await viewEngineResult.View.RenderAsync(viewContext);
            }
        }

        public ViewContext ViewContext
        {
            get { return _viewContext; }
        }
    }
}
