using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using LibGit2Sharp;
using System.IO;

namespace Wyam.Modules.Git
{
    public class GitContributor : GitBase
    {
        private readonly string _metadataName;

        public GitContributor(string metadataName)
        {
            _metadataName = metadataName;
        }

        public GitContributor() : this("Contributors")
        {

        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var reposetoryLocation = Repository.Discover(context.InputFolder);
            if (reposetoryLocation == null)
                throw new ArgumentException("No git reposetory found");

            using (Repository reposetory = new Repository(reposetoryLocation))
            {

                var data = GetCommitInformation(reposetory);
                var lookup = data.ToLookup(x => x.Path.ToLower());
                return inputs.Select(x =>
                {
                    string relativePath = GetRelativePath(Path.GetDirectoryName(Path.GetDirectoryName(reposetoryLocation.ToLower())), x.Source.ToLower()); // yes we need to do it twice
                    if (!lookup.Contains(relativePath))
                        return x;

                    var commitsOfFile = lookup[relativePath].Distinct(new SingelUserDistinction()).ToArray();
                    return x.Clone(new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(_metadataName, commitsOfFile) });
                }).ToArray(); // Don't do it lazy or Commit is disposed.
            }
        }

        private string GetRelativePath(string reposotoryLocation, string source)
        {
            FilePath repositoryLocationPath = new FilePath(reposotoryLocation);
            FilePath sourcePath = new FilePath(source);
            FilePath relativePath = sourcePath.RelativeTo(repositoryLocationPath);
            return relativePath.ToWindowsPath();
        }


        private class SingelUserDistinction : IEqualityComparer<CommitInformation>
        {
            public bool Equals(CommitInformation x, CommitInformation y)
            {
                return x.Autor.Name == y.Autor.Name;
            }

            public int GetHashCode(CommitInformation obj)
            {
                return obj.Autor.Name.GetHashCode();
            }
        }

    }
}
