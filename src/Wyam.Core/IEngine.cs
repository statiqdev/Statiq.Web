using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;

namespace Wyam.Core
{
    /// <summary>
    /// Provides an engine interface for use inside the configuration files. Any advanced configuration methods
    /// or properties should go in this interface to be made available to configuration scripts.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// The initial metadata that is used to populate every new document.
        /// </summary>
        IMetadataDictionary InitialMetadata { get; }

        /// <summary>
        /// Global execution metadata that is available from the execution context.
        /// </summary>
        IMetadataDictionary GlobalMetadata { get; }

        IPipelineCollection Pipelines { get; }

        IFileSystem FileSystem { get; }

        string ApplicationInput { get; set; }

        IDocumentFactory DocumentFactory { get; set; }

        ISettings Settings { get; }
    }
}
