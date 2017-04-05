using System;
using System.Collections.Generic;

namespace Wyam.Git
{
    /// <summary>
    /// Keys for use with the <see cref="GitCommits"/> and <see cref="GitContributors"/> modules.
    /// </summary>
    public static class GitKeys
    {
        /// <summary>
        /// The SHA of the commit.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Sha = nameof(Sha);

        /// <summary>
        /// The SHA of every parent commit.
        /// </summary>
        /// <type><see cref="IReadOnlyList{String}"/></type>
        public const string Parents = nameof(Parents);

        /// <summary>
        /// The name of the author.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string AuthorName = nameof(AuthorName);

        /// <summary>
        /// The email of the author.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string AuthorEmail = nameof(AuthorEmail);

        /// <summary>
        /// The date of the author signature.
        /// </summary>
        /// <type><see cref="DateTimeOffset"/></type>
        public const string AuthorWhen = nameof(AuthorWhen);

        /// <summary>
        /// The name of the committer.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string CommitterName = nameof(CommitterName);

        /// <summary>
        /// The email of the committer.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string CommitterEmail = nameof(CommitterEmail);

        /// <summary>
        /// The date of the committer signature.
        /// </summary>
        /// <type><see cref="DateTimeOffset"/></type>
        public const string CommitterWhen = nameof(CommitterWhen);

        /// <summary>
        /// The commit message.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Message = nameof(Message);

        /// <summary>
        /// All commit entries. The key is the path of the file and the value is the status of the file within the commit.
        /// </summary>
        /// <type><c>IReadOnlyDictionary&lt;string,string&gt;</c></type>
        public const string Entries = nameof(Entries);

        /// <summary>
        /// The sequence of commits for the input document if <c>ForEachInputDocument()</c> was called (and an alternate
        /// metadata key was not provided).
        /// </summary>
        /// <type><see cref="IReadOnlyList{IDocument}"/></type>
        public const string Commits = nameof(Commits);

        /// <summary>
        /// The name of the contributor.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string ContributorName = nameof(ContributorName);

        /// <summary>
        /// The email of the contributor.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string ContributorEmail = nameof(ContributorEmail);

        /// <summary>
        /// A document representing each commit by this contributor that contains the metadata specified in <see cref="GitCommits"/>.
        /// </summary>
        /// <type><see cref="IReadOnlyList{IDocument}"/></type>
        public const string Contributors = nameof(Contributors);
    }
}