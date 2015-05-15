// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the default <see cref="IRazorViewFactory"/> implementation that creates
    /// <see cref="RazorView"/> instances with a given <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorViewFactory : IRazorViewFactory
    {
        private readonly IViewStartProvider _viewStartProvider;

        /// <summary>
        /// Initializes a new instance of RazorViewFactory
        /// </summary>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        public RazorViewFactory(IViewStartProvider viewStartProvider)
        {
            _viewStartProvider = viewStartProvider;
        }

        /// <inheritdoc />
        public IView GetView([NotNull] IRazorViewEngine viewEngine,
                             [NotNull] IRazorPage page,
                             bool isPartial)
        {
            var razorView = new RazorView(viewEngine, _viewStartProvider, page, isPartial);
            return razorView;
        }
    }
}