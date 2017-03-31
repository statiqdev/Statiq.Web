using System;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Stores global settings that control behavior and execution.
    /// </summary>
    /// <metadata cref="Keys.Host" usage="Setting" />
    /// <metadata cref="Keys.LinksUseHttps" usage="Setting" />
    /// <metadata cref="Keys.LinkRoot" usage="Setting" />
    /// <metadata cref="Keys.LinkHideIndexPages" usage="Setting" />
    /// <metadata cref="Keys.LinkHideExtensions" usage="Setting" />
    /// <metadata cref="Keys.UseCache" usage="Setting" />
    /// <metadata cref="Keys.CleanOutputPath" usage="Setting" />
    /// <metadata cref="Keys.DateTimeInputCulture" usage="Setting" />
    /// <metadata cref="Keys.DateTimeDisplayCulture" usage="Setting" />
    public interface ISettings : IMetadataDictionary, IReadOnlySettings
    {
        /// <summary>
        /// Gets or sets the host to use when generating links.
        /// </summary>
        /// <value>
        /// The link host.
        /// </value>
        [Obsolete]
        new string Host { get; set; }

        /// <summary>
        /// Indicates if generated links should use HTTPS
        /// instead of HTTP as the scheme.
        /// </summary>
        /// <value>
        /// <c>true</c> if HTTPS should be used.
        /// </value>
        [Obsolete]
        new bool LinksUseHttps { get; set; }

        /// <summary>
        /// Gets or sets the default root path to use when generating links.
        /// </summary>
        /// <value>
        /// The link root.
        /// </value>
        [Obsolete]
        new DirectoryPath LinkRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide index pages by default when generating links.
        /// </summary>
        /// <value>
        /// <c>true</c> if index pages should be hidden; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        new bool LinkHideIndexPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide ".html" and ".htm" extensions by default when generating links.
        /// </summary>
        /// <value>
        /// <c>true</c> if extensions should be hidden; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        new bool LinkHideExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether caching should be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if caching should be used; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        new bool UseCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to clean the output path on each execution.
        /// </summary>
        /// <value>
        /// <c>true</c> if the output path should be cleaned; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        new bool CleanOutputPath { get; set; }
    }
}
