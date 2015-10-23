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
    /// This Module Creates a Page for every Autor in the Git reposetory. Adding Metadata with Information about the Autor.
    /// </summary>
    public class GitAutors : GitBase
    {

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var reposetoryLocation = Repository.Discover(context.InputFolder);
            if (reposetoryLocation == null)
                throw new ArgumentException("No git reposetory found");

            using (Repository reposetory = new Repository(reposetoryLocation))
            {
                IEnumerable<CommitInformation> data = GetCommitInformation(reposetory);

                var lookup = data.ToLookup(x => x.Autor.Name);

                var newDocuments = lookup.Select(x => context.GetNewDocument(new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("Contrebutor", x.Key),
                    new KeyValuePair<string, object>("CommitInformations", x.ToArray())
                })).ToArray();// Don't do it lazy or Commit is disposed.
                return newDocuments;
            }
        }

    }
}
