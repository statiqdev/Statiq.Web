// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor
{
    public class RazorView : IView
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly IViewStartProvider _viewStartProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="RazorView"/>
        /// </summary>
        /// <param name="viewEngine">The <see cref="IRazorViewEngine"/> used to locate Layout pages.</param>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// <param name="razorPage">The <see cref="IRazorPage"/> instance to execute.</param>
        /// <param name="isPartial">Determines if the view is to be executed as a partial.</param>
        /// pages</param>
        public RazorView(IRazorViewEngine viewEngine,
                         IViewStartProvider viewStartProvider,
                         IRazorPage razorPage,
                         bool isPartial)
        {
            _viewEngine = viewEngine;
            _viewStartProvider = viewStartProvider;
            RazorPage = razorPage;
            IsPartial = isPartial;
        }

        /// <inheritdoc />
        public string Path
        {
            get { return RazorPage.Path; }
        }

        /// <summary>
        /// Gets <see cref="IRazorPage"/> instance that the views executes on.
        /// </summary>
        public IRazorPage RazorPage { get; private set; }

        /// <summary>
        /// Gets a value that determines if the view is executed as a partial.
        /// </summary>
        public bool IsPartial { get; private set; }

        /// <inheritdoc />
        public virtual async Task RenderAsync([NotNull] ViewContext context)
        {
            // Partials don't execute _ViewStart pages, but may execute Layout pages if the Layout property
            // is explicitly specified in the page.
            var bodyWriter = await RenderPageAsync(RazorPage, context, executeViewStart: !IsPartial);
            await RenderLayoutAsync(context, bodyWriter);
        }

        private async Task<StringCollectionTextWriter> RenderPageAsync(IRazorPage page,
                                                                ViewContext context,
                                                                bool executeViewStart)
        {
            var bufferedWriter = new StringCollectionTextWriter(context.Writer.Encoding);
            var writer = (TextWriter)bufferedWriter;
            
            // The writer for the body is passed through the ViewContext, allowing things like HtmlHelpers
            // and ViewComponents to reference it.
            var oldWriter = context.Writer;
            var oldFilePath = context.ExecutingFilePath;
            context.Writer = writer;
            context.ExecutingFilePath = page.Path;

            try
            {
                if (executeViewStart)
                {
                    // Execute view starts using the same context + writer as the page to render.
                    await RenderViewStartAsync(context);
                }

                await RenderPageCoreAsync(page, context);
                return bufferedWriter;
            }
            finally
            {
                context.Writer = oldWriter;
                context.ExecutingFilePath = oldFilePath;
                writer.Dispose();
            }
        }

        private async Task RenderPageCoreAsync(IRazorPage page, ViewContext context)
        {
            page.IsPartial = IsPartial;
            page.ViewContext = context;

            await page.ExecuteAsync();
        }

        private async Task RenderViewStartAsync(ViewContext context)
        {
            var viewStarts = _viewStartProvider.GetViewStartPages(RazorPage.Path);

            string layout = null;
            var oldFilePath = context.ExecutingFilePath;
            try
            {
                foreach (var viewStart in viewStarts)
                {
                    context.ExecutingFilePath = viewStart.Path;
                    // Copy the layout value from the previous view start (if any) to the current.
                    viewStart.Layout = layout;
                    await RenderPageCoreAsync(viewStart, context);
                    layout = viewStart.Layout;
                }
            }
            finally
            {
                context.ExecutingFilePath = oldFilePath;
            }

            // Copy over interesting properties from the ViewStart page to the entry page.
            RazorPage.Layout = layout;
        }

        private async Task RenderLayoutAsync(ViewContext context,
                                             StringCollectionTextWriter bodyWriter)
        {
            // A layout page can specify another layout page. We'll need to continue
            // looking for layout pages until they're no longer specified.
            var previousPage = RazorPage;
            var renderedLayouts = new List<IRazorPage>();
            while (!string.IsNullOrEmpty(previousPage.Layout))
            {
                var layoutPage = GetLayoutPage(context, previousPage.Layout);

                // Notify the previous page that any writes that are performed on it are part of sections being written
                // in the layout.
                previousPage.IsLayoutBeingRendered = true;
                layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
                layoutPage.RenderBodyDelegate = bodyWriter.CopyTo;
                bodyWriter = await RenderPageAsync(layoutPage, context, executeViewStart: false);

                renderedLayouts.Add(layoutPage);
                previousPage = layoutPage;
            }

            // Ensure all defined sections were rendered or RenderBody was invoked for page without defined sections.
            foreach (var layoutPage in renderedLayouts)
            {
                layoutPage.EnsureRenderedBodyOrSections();
            }

            await bodyWriter.CopyToAsync(context.Writer);
        }

        private IRazorPage GetLayoutPage(ViewContext context, string layoutPath)
        {
            var layoutPageResult = _viewEngine.FindPage(context, layoutPath);
            if (layoutPageResult.Page == null)
            {
                throw new InvalidOperationException("Cannot locate layout " + layoutPath);
            }

            var layoutPage = layoutPageResult.Page;
            return layoutPage;
        }
    }
}
