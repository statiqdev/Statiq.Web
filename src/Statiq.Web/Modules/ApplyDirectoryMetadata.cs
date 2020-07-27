using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Applies directory metadata documents to the input documents. Make sure this module
    /// is executed before applying local metadata like front matter or it will get overwritten
    /// by the directory metadata.
    /// </summary>
    public class ApplyDirectoryMetadata : SyncModule
    {
        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            // Figure out relative input paths for all metadata documents
            ImmutableDictionary<string, ImmutableArray<DirectoryMetadataData>> directoryMetadata = context
                .Outputs
                .FromPipeline(nameof(DirectoryMetadata))
                .GroupBy(x => x.Source.Parent.GetRelativeInputPath().FullPath)
                .ToImmutableDictionary(
                    x => x.Key,
                    x => x
                        .Select(doc => new DirectoryMetadataData
                        {
                            Recursive = doc.GetBool(WebKeys.Recursive, true),
                            Metadata = doc.GetRawEnumerable().ToImmutableArray()
                        })
                        .ToImmutableArray());

            // Iterate through all input documents in parallel
            return context.Inputs.Select(input =>
            {
                IDocument merged = input;

                // Get all matching directory metadata documents
                NormalizedPath path = input.Source.Parent.GetRelativeInputPath();
                Stack<DirectoryMetadataData> matchStack = new Stack<DirectoryMetadataData>();
                bool local = true;
                while (!path.IsNull)
                {
                    if (directoryMetadata.TryGetValue(path.FullPath, out ImmutableArray<DirectoryMetadataData> matches))
                    {
                        foreach (DirectoryMetadataData match in matches.Where(x => local || x.Recursive))
                        {
                            matchStack.Push(match);
                        }
                    }
                    if (path.IsNullOrEmpty)
                    {
                        break;
                    }
                    local = false;
                    path = path.Parent;
                }

                // Apply matches if there are any
                while (matchStack.Count > 0)
                {
                    DirectoryMetadataData match = matchStack.Pop();
                    merged = merged.Clone(match.Metadata);
                }

                return merged;
            });
        }

        private class DirectoryMetadataData
        {
            public bool Recursive { get; set; }
            public ImmutableArray<KeyValuePair<string, object>> Metadata { get; set; }
        }
    }
}