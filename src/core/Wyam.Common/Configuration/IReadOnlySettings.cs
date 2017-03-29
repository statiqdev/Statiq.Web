using System;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// Settings that can be globally set.
    /// </summary>
    public interface IReadOnlySettings : IMetadata
    {
        /// <summary>
        /// Gets the host to use when generating links.
        /// </summary>
        /// <value>
        /// The link host.
        /// </value>
        [Obsolete]
        string Host { get; }

        /// <summary>
        /// Indicates if generated links should use HTTPS
        /// instead of HTTP as the scheme.
        /// </summary>
        /// <value>
        /// <c>true</c> if HTTPS should be used.
        /// </value>
        [Obsolete]
        bool LinksUseHttps { get; }

        /// <summary>
        /// Gets the default root path to use when generating links.
        /// </summary>
        /// <value>
        /// The link root.
        /// </value>
        [Obsolete]
        DirectoryPath LinkRoot { get; }

        /// <summary>
        /// Gets a value indicating whether to hide index pages by default when generating links.
        /// </summary>
        /// <value>
        /// <c>true</c> if index pages should be hidden; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        bool LinkHideIndexPages { get; }

        /// <summary>
        /// Gets a value indicating whether to hide ".html" and ".htm" extensions by default when generating links.
        /// </summary>
        /// <value>
        /// <c>true</c> if extensions should be hidden; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        bool LinkHideExtensions { get; }

        /// <summary>
        /// Gets a value indicating whether caching should be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if caching should be used; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        bool UseCache { get; }

        /// <summary>
        /// Gets a value indicating whether to clean the output path on each execution.
        /// </summary>
        /// <value>
        /// <c>true</c> if the output path should be cleaned; otherwise, <c>false</c>.
        /// </value>
        [Obsolete]
        bool CleanOutputPath { get; }
    }
}
