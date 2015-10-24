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

            var metadataDictinary = inputs.Select(x =>
            {
                var found = _metadataFile
                .Select((y, index) => new { Index = index, MetadataFileEntry = y })
                .FirstOrDefault(y => y.MetadataFileEntry.MetadataFileName.Invoke<bool>(x, context));
                if (found == null)
                    return null;
                return new
                {
                    Priority = found.Index,
                    Path = Path.GetDirectoryName(x.Source),
                    found.MetadataFileEntry,
                    x.Metadata
                };
            })
            .Where(x => x != null)
            .ToLookup(x => x.Path)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Priority).ToArray());

            // Apply Metadata

            return inputs
                .Where(x => _preserveMetadataFiles || !(_metadataFile.Any(isMetadata => isMetadata.MetadataFileName.Invoke<bool>(x, context)))) // ignore files that define Metadata if not preserved
                .Select(x =>
                {
                    // First add the inhired Metadata to temp dictenary.
                    string dir = Path.GetDirectoryName(x.Source);
                    List<string> sourcePathes = new List<string>();
                    while (dir.StartsWith(context.InputFolder))
                    {
                        sourcePathes.Add(dir);
                        dir = Path.GetDirectoryName(dir);
                    }

                    HashSet<string> overriddenKeys = new HashSet<string>(); // we need to know which keys we may override if they are overriden.
                    List<KeyValuePair<string, object>> newMetadataKey = new List<KeyValuePair<string, object>>();

                    bool firstLevel = true;
                    foreach (var path in sourcePathes)
                    {
                        if (metadataDictinary.ContainsKey(path))
                        {

                            foreach (var metadataEntry in metadataDictinary[path])
                            {
                                if (!firstLevel && !metadataEntry.MetadataFileEntry.Inherited)
                                    continue; // If we are not in the same directory and inherited isne't activeated 

                                foreach (var keyValuePair in metadataEntry.Metadata)
                                {
                                    if (overriddenKeys.Contains(keyValuePair.Key))
                                        continue; // The value was already written.

                                    if (x.Metadata.ContainsKey(keyValuePair.Key)
                                        && !metadataEntry.MetadataFileEntry.Override)
                                        continue; // The value alredy exists and this metadatafile has no override

                                    // We can add the value.
                                    overriddenKeys.Add(keyValuePair.Key); // no other MetadataFile may overide it.

                                    newMetadataKey.Add(keyValuePair);
                                }
                            }
                        }
                        firstLevel = false;
                    }

                    if (newMetadataKey.Any())
                        return x.Clone(newMetadataKey);
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
