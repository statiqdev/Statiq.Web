using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Common.Execution
{
    public interface IReadOnlyOutputSettings
    {
        /// <summary>
        /// Gets the host to use when generating links.
        /// </summary>
        /// <value>
        /// The link host.
        /// </value>
        string LinkHost { get; }

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
        /// Gets a value indicating whether to hide web extensions by default
        /// when generating links with <see cref="NormalizedPath.ToLink"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if web extensions should be hidden; otherwise, <c>false</c>.
        /// </value>
        bool LinkHideWebExtensions { get; }
    }
}
