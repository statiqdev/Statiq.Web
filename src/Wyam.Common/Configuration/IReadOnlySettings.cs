using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.IO;

namespace Wyam.Common.Configuration
{
    public interface IReadOnlySettings
    {
        /// <summary>
        /// Gets the host to use when generating links.
        /// </summary>
        /// <value>
        /// The link host.
        /// </value>
        string Host { get; }

        /// <summary>
        /// Gets the default root path to use when generating links 
        /// with <see cref="NormalizedPath.ToLink"/>
        /// </summary>
        /// <value>
        /// The link root.
        /// </value>
        DirectoryPath LinkRoot { get; }

        /// <summary>
        /// Gets a value indicating whether to hide index pages by default
        /// when generating links with <see cref="NormalizedPath.ToLink"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if index pages should be hidden; otherwise, <c>false</c>.
        /// </value>
        bool LinkHideIndexPages { get; }

        /// <summary>
        /// Gets a value indicating whether to hide extensions by default
        /// when generating links with <see cref="NormalizedPath.ToLink"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if extensions should be hidden; otherwise, <c>false</c>.
        /// </value>
        bool LinkHideExtensions { get; }

        /// <summary>
        /// Gets a value indicating whether caching should be used.
        /// </summary>
        /// <value>
        /// <c>true</c> if caching should be used; otherwise, <c>false</c>.
        /// </value>
        bool UseCache { get; }

        /// <summary>
        /// Gets a value indicating whether to clean the output path on each execution.
        /// </summary>
        /// <value>
        /// <c>true</c> if the output path should be cleaned; otherwise, <c>false</c>.
        /// </value>
        bool CleanOutputPath { get; }
    }
}
