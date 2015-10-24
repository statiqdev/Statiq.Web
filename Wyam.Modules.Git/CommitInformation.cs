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
        public Author Author { get; }
        public Author Committer { get; }
        public string Message { get; }
        public ChangeKind Status { get; }
        public string Path { get; private set; }

        public CommitInformation(ChangeKind status, Author author, Author committer, string message, string path)
        {
            this.Status = status;
            this.Author = author;
            this.Committer = committer;
            this.Message = message;
            this.Path = path;
        }
    }
}
