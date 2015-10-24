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
        public Autor Autor { get; }
        public Autor Committer { get; }
        public string Message { get; }
        public ChangeKind Status { get; }
        public string Path { get; private set; }

        public CommitInformation(ChangeKind status, Autor autor, Autor committer, string message, string path)
        {
            this.Status = status;
            this.Autor = autor;
            this.Committer = committer;
            this.Message = message;
            this.Path = path;
        }
    }
}
