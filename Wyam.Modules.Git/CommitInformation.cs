using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Wyam.Modules.Git
{
    /// <summary>
    /// Stores information per file per Commit.
    /// </summary>
    /// <remarks>
    /// Two commits with two changed files and three changed files will
    /// result in five <see cref="CommitInformation"/>.
    /// </remarks>
    public class CommitInformation
    {
        /// <summary>
        /// The hash of the commit.
        /// </summary>
        public string Sha { get; }
        /// <summary>
        /// The parent commits.
        /// </summary>
        public IEnumerable<string> Parents { get; }
        /// <summary>
        /// The Author of the changes.
        /// </summary>
        public Author Author { get; }
        /// <summary>
        /// The time the changes where made.
        /// </summary>
        public DateTimeOffset AuthorDateTime { get; }
        /// <summary>
        /// The person that commited the file.
        /// </summary>
        public Author Committer { get; }
        /// <summary>
        /// The time the commit was commited.
        /// </summary>
        public DateTimeOffset CommitterDateTime { get; }
        /// <summary>
        /// The commit message.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// Describes what happend with the file.
        /// </summary>
        public ChangeKind Status { get; }
        /// <summary>
        /// The rath relative to the reposetory root.
        /// </summary>
        public string Path { get; private set; }



        public CommitInformation(ChangeKind status, Author author, DateTimeOffset authorTime, Author committer, DateTimeOffset commitTime, string message, string path, string sha, IEnumerable<string> parents)
        {
            this.Status = status;
            this.Author = author;
            this.AuthorDateTime = authorTime;
            this.Committer = committer;
            this.CommitterDateTime = commitTime;
            this.Message = message;
            this.Path = path;
            this.Sha = sha;
            this.Parents = parents.ToArray();
        }
    }
}
