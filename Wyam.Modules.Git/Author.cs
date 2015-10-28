using LibGit2Sharp;
using System;

namespace Wyam.Modules.Git
{
    public class Author
    {
        public string Name { get; }
        public string Email { get; }


        internal Author(Signature committer)
        {
            Name = committer.Name;
            Email = committer.Email;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || !(obj is Author))
            {
                return false;
            }
            var otherAuthor = obj as Author;
            return this.Email.Equals(otherAuthor.Email);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.Email.GetHashCode();
        }

        public override string ToString()
        {
            return String.IsNullOrWhiteSpace(Name) ? Email : $"{Name} <{Email}>";
        }
    }
}