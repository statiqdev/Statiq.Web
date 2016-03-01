using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public interface IFileSystem : IReadOnlyFileSystem
    {
        new bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Gets the file providers.
        /// </summary>
        /// <value>
        /// The file providers.
        /// </value>
        new IFileProviderCollection FileProviders { get; }

        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        new DirectoryPath RootPath { get; set; }

        /// <summary>
        /// Gets the input paths collection which can be used
        /// to add or remove input paths.
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        new PathCollection<DirectoryPath> InputPaths { get; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        new DirectoryPath OutputPath { get; set; }
    }
}
