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

namespace Wyam.Core.Modules
{
    /// <summary>
    /// This Module allows to specify metadata for Multiple documents in one file.
    /// </summary>
    /// <remarks>
    /// This Module uses the folder structure of the Source property in the documents to
    /// determinate which files share there Metadata.
    /// 
    /// This can lead to unexpected behavior if Source is not a path (file system).
    /// 
    /// You can define multiple filenames that will be regarded as MetadataFiles.
    /// Use <see cref="WithMetadataFile(string, bool, bool)"/> to define some.
    /// 
    /// This Module uses whatever Metadata is present in the documents that
    /// are regarded as MetadataFiles. You can use e.g. Yaml to generate the Metadata.
    /// 
    /// Only Documents in the pipeline will be searched for MetadataFiles. So both the
    /// MetadataFiles and the documents that should get the Metadata should be processed by this Module.
    /// 
    /// The order you register new MetadataFilenames determents the priority which value
    /// will be used if there is a conflict. Use this list to find out which value will be assigned:
    /// 
    /// * Local value
    /// * MetadataFiles in the same location.
    ///   - The MetadataFiel that was registered first has the highest priority
    /// * Metadatafiles in parent directory (only if those have the inherited switch set)
    ///   - The MetadataFile that is less levels above has the highest priority
    /// 
    /// For every MetadataFile you define you can pass multiple Options.
    ///  * **inherited** (default = <c>false</c>)
    ///                   If this is true metadata will be set on Documents in sub folders.
    /// 
    ///  * **override**  (default = <c>false</c>)
    ///                  Normally a local value has the highest priority. With this option set to
    ///                  true, Metadata in this MetadataFile can override local values in a document.
    ///                  But it will not override values from other MetadataFiles that have 
    ///                  a higher priority
    /// 
    /// By default files that define metadata will be filtered by this Module
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
        public DirectoryMeta WithMetadataFile(DocumentConfig metadataFileName, bool inherited = false, bool replace = false)
        {
            _metadataFile.Add(new MetaFileEntry(metadataFileName, inherited, replace));
            return this;
        }

        public DirectoryMeta WithMetadataFile(string metadataFileName, bool inherited = false, bool replace = false)
        {
            return WithMetadataFile((x, y) => Path.GetFileName(x.Source) == metadataFileName, inherited, replace);
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
                    // First add the inhered Metadata to temp dictionary.
                    string dir = Path.GetDirectoryName(x.Source);
                    List<string> sourcePathes = new List<string>();
                    while (dir.StartsWith(context.InputFolder))
                    {
                        sourcePathes.Add(dir);
                        dir = Path.GetDirectoryName(dir);
                    }

                    HashSet<string> overriddenKeys = new HashSet<string>(); // we need to know which keys we may override if they are overridden.
                    List<KeyValuePair<string, object>> newMetadataKey = new List<KeyValuePair<string, object>>();

                    bool firstLevel = true;
                    foreach (var path in sourcePathes)
                    {
                        if (metadataDictinary.ContainsKey(path))
                        {

                            foreach (var metadataEntry in metadataDictinary[path])
                            {
                                if (!firstLevel && !metadataEntry.MetadataFileEntry.Inherited)
                                    continue; // If we are not in the same directory and inherited isn't activated 

                                foreach (var keyValuePair in metadataEntry.Metadata)
                                {
                                    if (overriddenKeys.Contains(keyValuePair.Key))
                                        continue; // The value was already written.

                                    if (x.Metadata.ContainsKey(keyValuePair.Key)
                                        && !metadataEntry.MetadataFileEntry.Replace)
                                        continue; // The value already exists and this MetadataFile has no override

                                    // We can add the value.
                                    overriddenKeys.Add(keyValuePair.Key); // no other MetadataFile may override it.

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
            public bool Replace { get; }

            public MetaFileEntry(DocumentConfig metadataFileName, bool inherited, bool replace)
            {
                this.MetadataFileName = metadataFileName;
                this.Inherited = inherited;
                this.Replace = replace;
            }
        }
    }
}
