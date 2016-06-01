// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.IO;
using Wyam.Razor.Microsoft.Framework.Internal;

namespace Wyam.Razor.Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class ViewStartProvider : IViewStartProvider
    {
        private readonly IRazorPageFactory _pageFactory;
        private readonly FilePath _viewStartPath;

        public ViewStartProvider(IRazorPageFactory pageFactory, FilePath viewStartPath)
        {
            _pageFactory = pageFactory;
            _viewStartPath = viewStartPath;
        }

        /// <inheritdoc />
        public IEnumerable<IRazorPage> GetViewStartPages([NotNull] string path)
        {
            var viewStartLocations = _viewStartPath == null
                ? ViewHierarchyUtility.GetViewStartLocations(path)
                : new [] {_viewStartPath.FullPath};
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