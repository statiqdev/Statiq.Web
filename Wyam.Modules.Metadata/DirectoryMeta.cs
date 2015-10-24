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
    public class DirectoryMeta : IModule
    {
        private bool _preserveMetadataFiles;
        private readonly List<MetaFileEntry> _metadataFile = new List<MetaFileEntry>();


        /// <summary>  
        /// This preserves the files that hold the directory metadata. Without this option theses files will be consumed by this module and will not be present in the following Modules.  
        /// </summary>  
        /// <returns>The current Object</returns>  
        public DirectoryMeta WithPreserveMetadataFiles()
        {
            _preserveMetadataFiles = true;
            return this;
        }



        /// <summary>
        /// This preserves the files that hold the directory metadata. Without this option theses files will be consumed by this module and will not be present in the following Modules.
        /// </summary>
        /// <returns>The current Object</returns>
        public DirectoryMeta WithMetadataFile(DocumentConfig metadataFileName, bool inherited = false, bool @override = false)
        {
            _metadataFile.Add(new MetaFileEntry(metadataFileName, inherited, @override));
            return this;
        }

        public DirectoryMeta WithMetadataFile(string metadataFileName, bool inherited = false, bool @override = false)
        {
            return WithMetadataFile((x, y) => Path.GetFileName(x.Source) == metadataFileName, inherited, @override);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            //Find MetadataFles
            var metadataDictinary = inputs.Where(x => _isLocalMetadata.Invoke<bool>(x, context)).Select(x => new { Path = Path.GetDirectoryName(x.Source), Metadata = x.Metadata }).ToDictionary(x => x.Path, x => x.Metadata);

            var metadataInhiredDictinary = inputs.Where(x => _isInheritMetadata.Invoke<bool>(x, context)).Select(x => new { Path = Path.GetDirectoryName(x.Source), Metadata = x.Metadata }).ToDictionary(x => x.Path, x => x.Metadata);
            // Apply Metadata

            return inputs
                .Where(x => _preserveMetadataFiles || !(_isLocalMetadata.Invoke<bool>(x, context) || _isInheritMetadata.Invoke<bool>(x, context))) // ignore files that define Metadata if not preserved
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

        private class MetaFileEntry
        {
            public bool Inherited { get; }
            public DocumentConfig MetadataFileName { get; }
            public bool Override { get; }

            public MetaFileEntry(DocumentConfig metadataFileName, bool inherited, bool @override)
            {
                this.MetadataFileName = metadataFileName;
                this.Inherited = inherited;
                this.Override = @override;
            }
        }
    }
}
