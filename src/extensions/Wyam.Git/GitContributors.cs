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
    /// Outputs documents and metadata for contributors in a Git repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module works in one of two ways. By default, a new document is output for each contributor in the
    /// repository. These output documents have the metadata documented below to describe each contributor. In
    /// this mode, all input documents are forgotten and only documents for each contributor are output.
    /// </para>
    /// <para>
    /// Alternatively, by calling <c>ForEachInputDocument()</c>, contributor data is added to every input document
    /// for which the repository contains an entry. The data is added as an <c>IDocument</c> sequence to the
    /// specified metadata key in the input document and each document in the sequence contains the same
    /// metadata that would have been added in the default mode. All input documents are output from this module
    /// (including those that didn't have commit information).
    /// </para>
    /// </remarks>
    /// <metadata name="ContributorName" type="string">The name of the contributor.</metadata>
    /// <metadata name="ContributorEmail" type="string">The email of the contributor.</metadata>
    /// <metadata name="Commits" type="IReadOnlyList&lt;IDocument&gt;">
    /// A document representing each commit by this contributor that contains the metadata specified in <see cref="GitCommits"/>.
    /// </metadata>
    /// <category>Metadata</category>
    public class GitContributors : GitModule
    {
        private bool _authors = true;
        private bool _committers = true;
        private string _contributorsMetadataKey;

        /// <summary>
        /// Gets authors from the repository the <c>InputFolder</c> is a part of.
        /// </summary>
        public GitContributors()
        {
        }

        /// <summary>
        /// Gets authors from the repository the specified path is a part of.
        /// </summary>
        /// <param name="repositoryPath">The repository path.</param>
        public GitContributors(DirectoryPath repositoryPath)
            : base(repositoryPath)
        {
        }

        /// <summary>
        /// Specifies that authors should be included.
        /// </summary>
        /// <param name="authors">If set to <c>true</c> (the default), authors are included in the output.</param>
        /// <returns>The current module instance.</returns>
        public GitContributors WithAuthors(bool authors = true)
        {
            _authors = authors;
            return this;
        }

        /// <summary>
        /// Specifies that committers should be included.
        /// </summary>
        /// <param name="committers">If set to <c>true</c> (the default), committers are included in the output.</param>
        /// <returns>The current module instance.</returns>
        public GitContributors WithCommitters(bool committers = true)
        {
            _committers = committers;
            return this;
        }

        /// <summary>
        /// Specifies that contributor information should be added to each input document.
        /// </summary>
        /// <param name="contributorsMetadataKey">The metadata key to set for contributor information.</param>
        /// <returns>The current module instance.</returns>
        public GitContributors ForEachInputDocument(string contributorsMetadataKey = GitKeys.Contributors)
        {
            _contributorsMetadataKey = contributorsMetadataKey;
            return this;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            ImmutableArray<IDocument> commitDocuments = GetCommitDocuments(inputs, context);

            // Build up the mapping (key = email, value = name/commits)
            Dictionary<string, Tuple<string, List<IDocument>>> contributors
                = new Dictionary<string, Tuple<string, List<IDocument>>>();
            foreach (IDocument commitDocument in commitDocuments)
            {
                string authorEmail = null;
                if (_authors)
                {
                    authorEmail = commitDocument.String(GitKeys.AuthorEmail);
                    Tuple<string, List<IDocument>> nameAndCommits;
                    if (!contributors.TryGetValue(authorEmail, out nameAndCommits))
                    {
                        nameAndCommits = new Tuple<string, List<IDocument>>(
                            commitDocument.String(GitKeys.AuthorName), new List<IDocument>());
                        contributors[authorEmail] = nameAndCommits;
                    }
                    nameAndCommits.Item2.Add(commitDocument);
                }
                if (_committers)
                {
                    string committerEmail = commitDocument.String(GitKeys.CommitterEmail);
                    if (committerEmail != authorEmail)
                    {
                        Tuple<string, List<IDocument>> nameAndCommits;
                        if (!contributors.TryGetValue(committerEmail, out nameAndCommits))
                        {
                            nameAndCommits = new Tuple<string, List<IDocument>>(
                                commitDocument.String(GitKeys.CommitterName), new List<IDocument>());
                            contributors[committerEmail] = nameAndCommits;
                        }
                        nameAndCommits.Item2.Add(commitDocument);
                    }
                }
            }

            // Iterate the contributors
            ImmutableArray<IDocument> contributorDocuments =
                contributors.Select(x => context.GetDocument(new MetadataItems
                {
                    new MetadataItem(GitKeys.ContributorEmail, x.Key),
                    new MetadataItem(GitKeys.ContributorName, x.Value.Item1),
                    new MetadataItem(GitKeys.Commits, x.Value.Item2.ToImmutableArray())
                })).ToImmutableArray();

            // Outputting contributors as new documents
            if (string.IsNullOrEmpty(_contributorsMetadataKey))
            {
                return contributorDocuments;
            }

            // Outputting contributor information for each document (with only commits for that document)
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
                ImmutableArray<IDocument> inputContributorDocuments = contributorDocuments
                    .Select(x => context.GetDocument(new MetadataItems
                    {
                        new MetadataItem(GitKeys.ContributorEmail, x[GitKeys.ContributorEmail]),
                        new MetadataItem(GitKeys.ContributorName, x[GitKeys.ContributorName]),
                        new MetadataItem(GitKeys.Commits, x.Get<IReadOnlyList<IDocument>>(GitKeys.Commits)
                            .Where(y => y.Get<IReadOnlyDictionary<FilePath, string>>(GitKeys.Entries).ContainsKey(relativePath))
                            .ToImmutableArray())
                    }))
                    .Where(x => x.Get<IReadOnlyList<IDocument>>(GitKeys.Commits).Count > 0)
                    .ToImmutableArray();
                return context.GetDocument(input, new MetadataItems
                {
                    new MetadataItem(_contributorsMetadataKey, inputContributorDocuments)
                });
            });
        }
    }
}
