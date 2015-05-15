// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Default implementation of <see cref="IRazorViewEngine"/>.
    /// </summary>
    /// <remarks>
    /// For <c>ViewResults</c> returned from controllers, views should be located in <see cref="ViewLocationFormats"/>
    /// by default. For the controllers in an area, views should exist in <see cref="AreaViewLocationFormats"/>.
    /// </remarks>
    public class RazorViewEngine : IRazorViewEngine
    {
        private const string ViewExtension = ".cshtml";

        private static readonly IEnumerable<string> _viewLocationFormats = new[]
        {
            "/Views/{0}" + ViewExtension,
            "/Views/Shared/{0}" + ViewExtension,
        };

        private readonly IRazorPageFactory _pageFactory;
        private readonly IRazorViewFactory _viewFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorViewEngine" /> class.
        /// </summary>
        /// <param name="pageFactory">The page factory used for creating <see cref="IRazorPage"/> instances.</param>
        public RazorViewEngine(IRazorPageFactory pageFactory,
                               IRazorViewFactory viewFactory)
        {
            _pageFactory = pageFactory;
            _viewFactory = viewFactory;
        }

        /// <summary>
        /// Gets the locations where this instance of <see cref="RazorViewEngine"/> will search for views.
        /// </summary>
        /// <remarks>
        /// The locations of the views returned from controllers that do not belong to an area.
        /// Locations are composite format strings (see http://msdn.microsoft.com/en-us/library/txafckwd.aspx),
        /// which contains following indexes:
        /// {0} - Action Name
        /// {1} - Controller Name
        /// The values for these locations are case-sensitive on case-senstive file systems.
        /// For example, the view for the <c>Test</c> action of <c>HomeController</c> should be located at
        /// <c>/Views/Home/Test.cshtml</c>. Locations such as <c>/views/home/test.cshtml</c> would not be discovered
        /// </remarks>
        public virtual IEnumerable<string> ViewLocationFormats
        {
            get { return _viewLocationFormats; }
        }

        // Gets a view that acts like a normal file-based view, but uses the provided content
        public ViewEngineResult GetView([NotNull] ViewContext context,
                                         string viewName,
                                         string content)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("viewName");
            }

            var pageResult = GetRazorPageResult(context, viewName, isPartial: false, content: content);
            return CreateViewEngineResult(pageResult, _viewFactory, isPartial: false);
        }

        /// <inheritdoc />
        public ViewEngineResult FindView([NotNull] ViewContext context,
                                         string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("viewName");
            }

            var pageResult = GetRazorPageResult(context, viewName, isPartial: false);
            return CreateViewEngineResult(pageResult, _viewFactory, isPartial: false);
        }

        /// <inheritdoc />
        public ViewEngineResult FindPartialView([NotNull] ViewContext context,
                                                string partialViewName)
        {
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("partialViewName");
            }

            var pageResult = GetRazorPageResult(context, partialViewName, isPartial: true);
            return CreateViewEngineResult(pageResult, _viewFactory, isPartial: true);
        }

        /// <inheritdoc />
        public RazorPageResult FindPage([NotNull] ViewContext context,
                                        string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                throw new ArgumentException("pageName");
            }

            return GetRazorPageResult(context, pageName, isPartial: true);
        }

        private RazorPageResult GetRazorPageResult(ViewContext context,
                                                   string pageName,
                                                   bool isPartial,
                                                   string content = null)
        {
            if (IsApplicationRelativePath(pageName))
            {
                var applicationRelativePath = pageName;
                if (!pageName.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase))
                {
                    applicationRelativePath += ViewExtension;
                }

                var page = _pageFactory.CreateInstance(applicationRelativePath, content);
                if (page != null)
                {
                    return new RazorPageResult(pageName, page);
                }

                return new RazorPageResult(pageName, new[] { pageName });
            }
            else
            {
                return LocatePageFromViewLocations(context, pageName, isPartial);
            }
        }

        private RazorPageResult LocatePageFromViewLocations(ViewContext context,
                                                            string pageName,
                                                            bool isPartial)
        {
            // Only use the area view location formats if we have an area token.
            var viewLocations = ViewLocationFormats;

            // 3. Use the expanded locations to look up a page.
            var searchedLocations = new List<string>();
            foreach (var path in viewLocations)
            {
                var transformedPath = string.Format(CultureInfo.InvariantCulture,
                                                    path,
                                                    pageName);
                var page = _pageFactory.CreateInstance(transformedPath);
                if (page != null)
                {
                    return new RazorPageResult(pageName, page);
                }

                searchedLocations.Add(transformedPath);
            }

            // 3b. We did not find a page for any of the paths.
            return new RazorPageResult(pageName, searchedLocations);
        }

        private ViewEngineResult CreateViewEngineResult(RazorPageResult result,
                                                        IRazorViewFactory razorViewFactory,
                                                        bool isPartial)
        {
            if (result.SearchedLocations != null)
            {
                return ViewEngineResult.NotFound(result.Name, result.SearchedLocations);
            }

            var view = razorViewFactory.GetView(this, result.Page, isPartial);
            return ViewEngineResult.Found(result.Name, view);
        }

        private static bool IsApplicationRelativePath(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            return name[0] == '~' || name[0] == '/';
        }
    }
}
