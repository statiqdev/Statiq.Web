﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Wyam.Abstractions;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc
{
    public class ViewContext
    {
        private DynamicViewData _viewBag;

        /// <summary>
        /// Initializes a new instance of <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="view">The <see cref="IView"/> being rendered.</param>
        /// <param name="viewData">The <see cref="ViewDataDictionary"/>.</param>
        /// <param name="tempData">The <see cref="ITempDataDictionary"/>.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to render output to.</param>
        public ViewContext(
            [NotNull] IView view,
            [NotNull] ViewDataDictionary viewData,
            [NotNull] TextWriter writer,
            IMetadata metadata,
            IExecutionContext executionContext,
            IViewEngine viewEngine)
        {
            View = view;
            ViewData = viewData;
            Writer = writer;
            Metadata = metadata;
            ExecutionContext = executionContext;
            ViewEngine = viewEngine;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="viewContext">The <see cref="ViewContext"/> to copy values from.</param>
        /// <param name="view">The <see cref="IView"/> being rendered.</param>
        /// <param name="viewData">The <see cref="ViewDataDictionary"/>.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to render output to.</param>
        public ViewContext(
            [NotNull] ViewContext viewContext,
            [NotNull] IView view,
            [NotNull] ViewDataDictionary viewData,
            [NotNull] TextWriter writer)
        {

            Metadata = viewContext.Metadata;
            ExecutionContext = viewContext.ExecutionContext;
            ViewEngine = viewContext.ViewEngine;
            View = view;
            ViewData = viewData;
            Writer = writer;
        }

        /// <summary>
        /// Gets the dynamic view bag.
        /// </summary>
        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null)
                {
                    _viewBag = new DynamicViewData(() => ViewData);
                }

                return _viewBag;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IView"/> currently being rendered, if any.
        /// </summary>
        public IView View { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> used to write the output.
        /// </summary>
        public TextWriter Writer { get; set; }

        /// <summary>
        /// Gets or sets the path of the view file currently being rendered.
        /// </summary>
        /// <remarks>
        /// The rendering of a view may involve one or more files (e.g. _ViewStart, Layouts etc).
        /// This property contains the path of the file currently being rendered.
        /// </remarks>
        public string ExecutingFilePath { get; set; }

        internal IViewEngine ViewEngine { get; set; }

        public IMetadata Metadata { get; set; }

        public IExecutionContext ExecutionContext { get; set; }
    }
}
