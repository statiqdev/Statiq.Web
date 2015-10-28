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
using Wyam.Common.Configuration;

namespace Wyam.Modules.Git
{
    /// <summary>
    /// This Module adds an Array with all the CommitInformations that are related with this file to the Metadata.
    /// </summary>
    public class GitFileCommits : GitBase
    {
        private readonly string _metadataName;

        public GitFileCommits(string metadataName)
        {
            _metadataName = metadataName;
        }

        public GitFileCommits() : this("Commits")
        {

        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var repositoryLocation = Repository.Discover(context.InputFolder);
            if (repositoryLocation == null)
                throw new ArgumentException("No git repository found");

            using (Repository repository = new Repository(repositoryLocation))
            {

                var data = GetCommitInformation(repository);
                var lookup = data.ToLookup(x => x.Path.ToLower());
                return inputs.Select(x =>
                {
                    string relativePath = GetRelativePath(Path.GetDirectoryName(Path.GetDirectoryName(repositoryLocation.ToLower())), x.Source.ToLower()); // yes we need to do it twice
                    if (!lookup.Contains(relativePath))
                        return x;

                    var commitsOfFile = lookup[relativePath].ToArray();
     
                    return x.Clone(new[]
                    {
                        new KeyValuePair<string, object>(_metadataName, commitsOfFile)
                    });
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


        

    }
}
