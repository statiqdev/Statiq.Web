using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Common.Execution
{
    public interface IOutputSettings : IReadOnlyOutputSettings
    {
        /// <summary>
        /// Gets or sets the host to use when generating links.
        /// </summary>
        /// <value>
        /// The link host.
        /// </value>
        new string LinkHost { get; set; }

        /// <summary>
        /// Gets or sets the default root path to use when generating 
        /// links with <see cref="NormalizedPath.ToLink"/>
        /// </summary>
        /// <value>
        /// The link root.
        /// </value>
        new DirectoryPath LinkRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide index pages by default
        /// when generating links with <see cref="NormalizedPath.ToLink"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if index pages should be hidden; otherwise, <c>false</c>.
        /// </value>
        new bool LinkHideIndexPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide web extensions by default
        /// when generating links with <see cref="NormalizedPath.ToLink"/>.
        /// </summary>
        /// <value>
        /// <c>true</c> if web extensions should be hidden; otherwise, <c>false</c>.
        /// </value>
        new bool LinkHideWebExtensions { get; set; }
    }
}
