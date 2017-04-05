using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using LibGit2Sharp;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Git
{
    /// <summary>
    /// Outputs documents and metadata for commits in a Git repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module works in one of two ways. By default, a new document is output for each commit in the
    /// repository. These output documents have the metadata documented below to describe each commit. In
    /// this mode, all input documents are forgotten and only documents for each commit are output.
    /// </para>
    /// <para>
    /// Alternatively, by calling <c>ForEachInputDocument()</c>, commit data is added to every input document
    /// for which the repository contains an entry. The data is added as an <c>IDocument</c> sequence to the
    /// specified metadata key in the input document and each document in the sequence contains the same
    /// metadata that would have been added in the default mode. All input documents are output from this module
    /// (including those that didn't have commit information).
    /// </para>
    /// </remarks>
    /// <metadata cref="GitKeys.Sha" usage="Output"/>
    /// <metadata cref="GitKeys.Parents" usage="Output"/>
    /// <metadata cref="GitKeys.AuthorName" usage="Output"/>
    /// <metadata cref="GitKeys.AuthorEmail" usage="Output"/>
    /// <metadata cref="GitKeys.AuthorWhen" usage="Output"/>
    /// <metadata cref="GitKeys.CommitterName" usage="Output"/>
    /// <metadata cref="GitKeys.CommitterEmail" usage="Output"/>
    /// <metadata cref="GitKeys.CommitterWhen" usage="Output"/>
    /// <metadata cref="GitKeys.Message" usage="Output"/>
    /// <metadata cref="GitKeys.Entries" usage="Output"/>
    /// <metadata cref="GitKeys.Commits" usage="Output"/>
    /// <category>Metadata</category>
    public class GitCommits : GitModule
    {
        private string _commitsMetadataKey;

        /// <summary>
        /// Gets commits from the repository the <c>InputFolder</c> is a part of.
        /// </summary>
        public GitCommits()
        {
        }

        /// <summary>
        /// Gets commits from the repository the specified path is a part of.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        public GitCommits(DirectoryPath repositoryPath)
            : base(repositoryPath)
        {
        }

        /// <summary>
        /// Specifies that commit information should be added to each input document.
        /// </summary>
        /// <param name="commitsMetadataKey">The metadata key to set for commit information.</param>
        /// <returns>The current module instance.</returns>
        public GitCommits ForEachInputDocument(string commitsMetadataKey = GitKeys.Commits)
        {
            _commitsMetadataKey = commitsMetadataKey;
            return this;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<IDocument> commitDocuments = GetCommitDocuments(inputs, context);

            // Outputting commits as new documents
            if (string.IsNullOrEmpty(_commitsMetadataKey))
            {
                return commitDocuments;
            }

            // Outputting commit information for each document
            DirectoryPath repositoryPath = GetRepositoryPath(context);
            return inputs.AsParallel().Select(context, input =>
            {
                if (input.Source == null)
                {
                    return input;
                }
                FilePath relativePath = repositoryPath.GetRelativePath(input.Source);
                if (relativePath.Equals(input.Source))
                {
                    return input;
                }
                ImmutableArray<IDocument> inputCommitDocuments = commitDocuments
                    .Where(x =>
                    {
                        IReadOnlyDictionary<FilePath, string> entries = x.Get<IReadOnlyDictionary<FilePath, string>>(GitKeys.Entries);
                        return x.Get<IReadOnlyDictionary<FilePath, string>>(GitKeys.Entries).ContainsKey(relativePath);
                    })
                    .ToImmutableArray();
                return context.GetDocument(input, new MetadataItems
                {
                    new MetadataItem(_commitsMetadataKey, inputCommitDocuments)
                });
            });
        }
    }
}
