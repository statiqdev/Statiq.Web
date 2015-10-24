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
    /// determinate which files share there Metadata.
    /// 
    /// This can lead to unexpected behavior if Source is not a path (filesystem).
    /// 
    /// You can define multiple filenames that will be reagareded as MetadataFiles.
    /// Use <see cref="WithMetadataFile(string, bool, bool)"/> to define some.
    /// 
    /// This Module ueses whatever Metadata is present in the documents that
    /// are regareded as MetadataFiles. You can use e.g. Yaml to generate the Metadata.
    /// 
    /// The order you register new MetadataFilenames determns the priority which value
    /// will be used if there is a conflict. Use this list to find out which value will be assigned:
    /// 
    /// * Local value
    /// * MetadataFiles in the same locataion.
    ///   - The MetadataFiel that was registered first has the highest priority
    /// * Metadatafiles in parent firectorys (only if those have the inherited switch set)
    ///   - The MetadataFile that is less levels abouve has the highest priorety
    /// 
    /// For every MetadataFile you define you can pass multiple Options.
    ///  * **inherited** (default = <c>false</c>)
    ///                   If this is true metadata will be set on Documents in sub folders.
    /// 
    ///  * **override**  (default = <c>false</c>)
    ///                  Normaly a local value has the highest priorety. With this option set to
    ///                  true, Metadata in this MetadataFile can override local values in a document.
    ///                  But it will not override values from other MetadataFiles that have 
    ///                  a higher priorety
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
