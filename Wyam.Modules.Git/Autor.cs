using LibGit2Sharp;
using System;

namespace Wyam.Modules.Git
{
    public class Autor
    {
        public string Name { get; }
        public string Email { get; }
        public DateTimeOffset DateTime { get; }

        internal Autor(Signature committer)
        {
            Name = committer.Name;
            Email = committer.Email;
            DateTime = committer.When;
        }
    }
}