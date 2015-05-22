﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Wyam.Abstractions;
using Wyam.Modules.Razor.Microsoft.AspNet.Mvc.Rendering;
using Wyam.Modules.Razor.Microsoft.Framework.Internal;

namespace Wyam.Modules.Razor.Microsoft.AspNet.Mvc
{
    public class ViewContext
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ViewContext"/>.
        /// </summary>
        /// <param name="view">The <see cref="IView"/> being rendered.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to render output to.</param>
        public ViewContext(
            [NotNull] IView view,
            [NotNull] TextWriter writer)
        {
            View = view;
            Writer = writer;
        }

        /// <summary>
        /// Gets or sets the <see cref="IView"/> currently being rendered, if any.
        /// </summary>
        public IView View { get; set; }

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

        public IMetadata Metadata { get; set; }
    }
}
