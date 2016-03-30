using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules.IO;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Applies metadata from specified input documents to all input documents based on a directory hierarchy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module allows you to specify certain documents that contain common metadata for all other
    /// documents in the same directory (and optionally nested directories). It assumes that all input documents
    /// are generated from the file system (for example, from the <see cref="ReadFiles"/> module). In other words,
    /// both the documents that contain the common metadata and the documents to which the common metadata should
    /// be applied should be passed as inputs to this module.
    /// </para>
    /// <para>
    /// Documents that contain the common metadata are specified by file name using the <c>WithMetadataFile</c> method.
    /// You can specify more than one metadata file and/or metadata files at different levels in the directory
    /// hierarchy. If the same metadata key exists across multiple common metadata documents, the following can be
    /// used to determine which metadata value will get set in the target output documents:
    /// <list type="bullet">
    /// <item><description>
    /// Pre-existing metadata in the target document (common metadata will 
    /// not overwrite existing metadata unless the <c>replace</c> flag is set).
    /// </description></item>
    /// <item><description>
    /// Common metadata documents in the same directory as the target document 
    /// (those registered first have a higher priority).
    /// </description></item>
    /// <item><description>
    /// Common metadata documents in parent directories of the target document (but only if the <c>inherited</c> flag 
    /// is set and those closer to the target document have a higher priority).
    /// </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// By default, documents that are identified as containing common metadata will be filtered and won't be
    /// contained in the sequence of output documents. <c>PreserveMetadataFiles</c> can be used to change this behavior.
    /// </para>
    /// </remarks>
    /// <category>Metadata</category>
    public class DirectoryMeta : IModule
    {
        private bool _preserveMetadataFiles;
        private readonly List<MetaFileEntry> _metadataFile = new List<MetaFileEntry>();

        /// <summary>  
        /// Preserves the files that hold the common metadata and ensures they are included in the module output. Without this option, theses documents will 
        /// be consumed by this module and will not be present in the module output.  
        /// </summary>  
        public DirectoryMeta WithPreserveMetadataFiles()
        {
            _preserveMetadataFiles = true;
            return this;
        }

        /// <summary>
        /// Specifies a file name to use as common metadata using a delegate so that the common metadata document can be specific to the input document.
        /// </summary>
        /// <param name="metadataFileName">A delegate that returns a <c>bool</c> indicating if the current document contains the metadata you want to use.</param>
        /// <param name="inherited">If set to <c>true</c>, metadata from documents with this file name will be inherited by documents in nested directories.</param>
        /// <param name="replace">If set to <c>true</c>, metadata from this document will replace any existing metadata on the target document.</param>
        public DirectoryMeta WithMetadataFile(DocumentConfig metadataFileName, bool inherited = false, bool replace = false)
        {
            _metadataFile.Add(new MetaFileEntry(metadataFileName, inherited, replace));
            return this;
        }

        /// <summary>
        /// Specifies a file name to use as common metadata.
        /// </summary>
        /// <param name="metadataFileName">Name of the metadata file.</param>
        /// <param name="inherited">If set to <c>true</c>, metadata from documents with this file name will be inherited by documents in nested directories.</param>
        /// <param name="replace">If set to <c>true</c>, metadata from this document will replace any existing metadata on the target document.</param>
        public DirectoryMeta WithMetadataFile(FilePath metadataFileName, bool inherited = false, bool replace = false)
        {
            return WithMetadataFile((x, y) => x.Source != null && x.Source.FileName.Equals(metadataFileName), inherited, replace);
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            //Find metadata files
            var metadataDictionary = inputs
                .Where(input => input.Source != null)
                .Select(input =>
                {
                    var found = _metadataFile
                        .Select((y, index) => new
                        {
                            Index = index,
                            MetadataFileEntry = y
                        })
                        .FirstOrDefault(y => y.MetadataFileEntry.MetadataFileName.Invoke<bool>(input, context));
                    if (found == null)
                    {
                        return null;
                    }
                    return new
                    {
                        Priority = found.Index,
                        Path = input.Source.Directory.Collapse(),
                        found.MetadataFileEntry,
                        input.Metadata
                    };
                })
                .Where(x => x != null)
                .ToLookup(x => x.Path)
                .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Priority).ToArray());

            // Apply Metadata
            return inputs
                .Where(input => input.Source != null)
                .Where(input => _preserveMetadataFiles || !(_metadataFile.Any(isMetadata => isMetadata.MetadataFileName.Invoke<bool>(input, context)))) // ignore files that define Metadata if not preserved
                .Select(input =>
                {
                    // First add the inherited metadata to the temp dictionary
                    List<DirectoryPath> sourcePaths = new List<DirectoryPath>();
                    DirectoryPath inputPath = context.FileSystem.GetContainingInputPath(input.Source)?.Collapse();
                    if (inputPath != null)
                    {
                        DirectoryPath dir = input.Source.Directory.Collapse();
                        while (dir != null && dir.FullPath.StartsWith(inputPath.FullPath))
                        {
                            sourcePaths.Add(dir);
                            dir = dir.Parent;
                        }
                    }

                    HashSet<string> overriddenKeys = new HashSet<string>(); // we need to know which keys we may override if they are overridden.
                    List<KeyValuePair<string, object>> newMetadata = new List<KeyValuePair<string, object>>();

                    bool firstLevel = true;
                    foreach (DirectoryPath path in sourcePaths)
                    {
                        if (metadataDictionary.ContainsKey(path))
                        {
                            foreach (var metadataEntry in metadataDictionary[path])
                            {
                                if (!firstLevel && !metadataEntry.MetadataFileEntry.Inherited)
                                {
                                    continue; // If we are not in the same directory and inherited isn't activated 
                                }

                                foreach (KeyValuePair<string, object> keyValuePair in metadataEntry.Metadata)
                                {
                                    if (overriddenKeys.Contains(keyValuePair.Key))
                                    {
                                        continue; // The value was already written.
                                    }

                                    if (input.Metadata.ContainsKey(keyValuePair.Key)
                                        && !metadataEntry.MetadataFileEntry.Replace)
                                    {
                                        continue; // The value already exists and this MetadataFile has no override
                                    }

                                    // We can add the value.
                                    overriddenKeys.Add(keyValuePair.Key); // no other MetadataFile may override it.

                                    newMetadata.Add(keyValuePair);
                                }
                            }
                        }
                        firstLevel = false;
                    }

                    return newMetadata.Any() ? context.GetDocument(input, newMetadata) : input;
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
