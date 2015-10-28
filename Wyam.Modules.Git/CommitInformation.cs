using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Wyam.Modules.Git
{
    public class CommitInformation
    {
        public string Sha { get; }
        public IEnumerable<string> Parents { get; }
        public Author Author { get; }
        public DateTimeOffset AuthorDateTime { get; }
        public Author Committer { get; }
        public DateTimeOffset CommitterDateTime { get; }
        public string Message { get; }
        public ChangeKind Status { get; }
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
