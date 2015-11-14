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
    /// This Module Creates a Document for every Commit in the Git repository. Adding Metadata with
    /// Information about the Commit.
    /// 
    /// * This Module adds Metadata "Sha" of type <see cref="String"/> which is the hash of the commit.
    /// * This Module adds Metadata "CommitInformation" that is an Array of <see cref="CommitInformation"/>.
    ///   The array contains one entry for every File that was changed in this commit.
    /// </summary>
    public class GitCommits : GitBase
    {

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var repositoryLocation = Repository.Discover(context.InputFolder);
            if (repositoryLocation == null)
                throw new ArgumentException("No git repository found");

            using (Repository repository = new Repository(repositoryLocation))
            {
                IEnumerable<CommitInformation> data = GetCommitInformation(repository);

                var lookup = data.ToLookup(x => x.Sha);

                var newDocuments = lookup.Select(x => context.GetNewDocument(new [] {
                    new KeyValuePair<string, object>("Sha", x.Key),
                    new KeyValuePair<string, object>("CommitInformation", x.ToArray())
                })).ToArray();  // Don't do it lazy or Commit is disposed.
                return newDocuments;
            }
        }

    }
}
