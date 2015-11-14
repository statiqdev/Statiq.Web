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
    /// <summary>
    /// This module adds Metadata to every inputdocument containing every one who made changes to this file.
    /// The Key of the metadata is "Contributors" by default. But can be changed in the Constructor.
    /// The value of "Contributors" is an array of <see cref="CommitInformation"/> containeing the last commit of
    /// every Author.
    /// </summary>
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

                    var commitsOfFile = lookup[relativePath]
                        .GroupBy(y => y.Author)
                        .ToDictionary(y => y.Key,
                                    y => y.OrderByDescending(z => z.AuthorDateTime).First())
                        .Select(y => y.Value)
                        .ToArray();

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
