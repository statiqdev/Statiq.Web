using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Metadata
{
    /// <summary>
    /// This Module allows to specifiy metadata for Multiple documents in one file.
    /// </summary>
    /// <remarks>
    /// This Module uses the folder structor of the Source property in the documents to
    /// determinate which files share there Metadata. There are two ways to define this metadata.
    /// 
    ///  * localy:  by default every metadata defined in a file named "local.metadata"
    ///             will be added in all files in the same directory.
    /// 
    ///  * inhired: by default every metadata defined in a file named "inhired.metadata"
    ///             will be added in all files in the same directory and subdirectorys.
    /// 
    /// If metadata with the same key but different values is defined on multiple locations.
    /// 
    /// See following precedence list:
    ///  * defined in document
    ///  * defined localy
    ///  * defined inhired
    /// 
    /// If overide is set this change to following order:
    ///  * defined localy
    ///  * defined inhired
    ///  * defined in document
    /// 
    /// By default files that define metadata will be filterd by this Module
    /// and does not exist in the output. To change this use PreserveMetadataFiles.
    /// </remarks>
    public class DirectoryMetadata : IModule
    {
        private DocumentConfig _isLocalMetadata;
        private DocumentConfig _isInhiredMetadata;
        private bool _override;
        private bool _preserveMetadataFiles;


        public DirectoryMetadata()
        {
            _isLocalMetadata = (x, y) => Path.GetFileName(x.Source) == "local.metadata";
            _isInhiredMetadata = (x, y) => Path.GetFileName(x.Source) == "inhired.metadata";
        }

        /// <summary>
        /// This allows directory metadata to override file metadata.
        /// </summary>
        /// <returns>The current Object</returns>
        public DirectoryMetadata WithOverride()
        {
            _override = true;
            return this;
        }

        /// <summary>
        /// This preserves the files that hold the directory metadata. Without this option theses files will be consumed by this module and will not be present in the following Modules.
        /// </summary>
        /// <returns>The current Object</returns>
        public DirectoryMetadata WithPreserveMetadataFiles()
        {
            _preserveMetadataFiles = true;
            return this;
        }

        public DirectoryMetadata WithMetadata(DocumentConfig isMetadata)
        {
            _isLocalMetadata = isMetadata;
            return this;
        }
        public DirectoryMetadata WithInhiredMetadata(DocumentConfig isInhiredMetadata)
        {
            _isInhiredMetadata = isInhiredMetadata;
            return this;
        }

        /// <summary>
        /// This sets the filename where the metadata is found for a directory.
        /// The metadata applies only to the current directory.
        /// </summary>
        /// <param name="metadataFileName">The filename</param>
        /// <returns>The current Object</returns>
        public DirectoryMetadata WithMetadata(string metadataFileName)
        {
            _isLocalMetadata = (x, y) => Path.GetFileName(x.Source) == metadataFileName;
            return this;
        }

        /// <summary>
        /// This sets the filename where the metadata is found for a directory.
        /// The metadata applies to the current directory and all sub directorys.
        /// </summary>
        /// <param name="metadataFileName">The filename</param>
        /// <returns>The current Object</returns>
        public DirectoryMetadata WithInhiredMetadata(string inhiredMetadataFileName)
        {
            _isInhiredMetadata = (x, y) => Path.GetFileName(x.Source) == inhiredMetadataFileName;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            //Find MetadataFles
            var metadataDictinary = inputs.AsParallel().Where(x => _isLocalMetadata.Invoke<bool>(x, context)).Select(x => new { Path = Path.GetDirectoryName(x.Source), Metadata = x.Metadata }).ToDictionary(x => x.Path, x => x.Metadata);

            var metadataInhiredDictinary = inputs.AsParallel().Where(x => _isInhiredMetadata.Invoke<bool>(x, context)).Select(x => new { Path = Path.GetDirectoryName(x.Source), Metadata = x.Metadata }).ToDictionary(x => x.Path, x => x.Metadata);
            // Apply Metadata

            return inputs.AsParallel()
                .Where(x => _preserveMetadataFiles || !(_isLocalMetadata.Invoke<bool>(x, context) || _isInhiredMetadata.Invoke<bool>(x, context))) // ignore files that define Metadata if not preserved
                .Select(x =>
                {
                    // First add the inhired Metadata to temp dictenary.
                    string dir = Path.GetDirectoryName(x.Source);
                    List<string> metadataPathes = new List<string>();
                    while (dir.StartsWith(context.InputFolder))
                    {
                        if (metadataInhiredDictinary.ContainsKey(dir))
                        {
                            metadataPathes.Add(dir);
                        }
                        dir = Path.GetDirectoryName(dir);
                    }
                    metadataPathes.Reverse(); // starting with the top most directory, so subdirectorys overide higher ones.
                    Dictionary<string, object> newMetadata = new Dictionary<string, object>();
                    foreach (var metadataFile in metadataPathes)
                    {
                        foreach (var keyValuePair in metadataInhiredDictinary[metadataFile])
                        {
                            newMetadata[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }

                    //Seccond Add the nonInhiredMetadata to Dictenary
                    dir = Path.GetDirectoryName(x.Source);
                    if (metadataDictinary.ContainsKey(dir))
                    {
                        foreach (var keyValuePair in metadataDictinary[dir])
                        {
                            newMetadata[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }

                    // Check Overide Condition
                    var metadata = newMetadata as IEnumerable<KeyValuePair<string, object>>;
                    if (!_override) // Wir löschen alle Einträge welche bereits in der Datei vorhanden sind.
                    {
                        metadata = metadata.Where(m => !x.ContainsKey(m.Key));
                    }

                    if (metadata.Any())
                        return x.Clone(metadata);
                    return x;
                });
        }

    }
}
