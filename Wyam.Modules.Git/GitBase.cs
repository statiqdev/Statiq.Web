using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Modules;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Git
{
    public abstract class GitBase : IModule
    {
        protected static IEnumerable<CommitInformation> GetCommitInformation(Repository reposetory)
        {
            return reposetory.Commits
                .Select(c => CompareTrees(reposetory, c).Select(x => new { ChangeFile = x, Commit = c }))
                .SelectMany(x => x)
                .Select(x => new CommitInformation(x.ChangeFile.Status, new Author(x.Commit.Author), x.Commit.Author.When, new Author(x.Commit.Committer), x.Commit.Author.When, x.Commit.Message, x.ChangeFile.Path, x.Commit.Sha, x.Commit.Parents.Select(p => p.Sha)));
        }

        protected static IEnumerable<ChangeFile> CompareTrees(Repository repo, Commit toCheck)
        {
            Tree commitTree = toCheck.Tree; // Main Tree
            var parentCommitTrees = toCheck.Parents.Select(x => x.Tree).ToList(); // Parent Tree

            var patch = parentCommitTrees.Select(x => repo.Diff.Compare<Patch>(x, commitTree)).SelectMany(x => x); // Difference

            if (!parentCommitTrees.Any()) // Don't use Patch for commits without parents.
            {
                foreach (var ptc in TraverseTree(commitTree))
                {
                    yield return new ChangeFile() { Path = ptc, Status = ChangeKind.Added };
                }
            }
            else if (!parentCommitTrees.Skip(1).Any()) // Merge doesn't count to History. So everything that has mor than one parent will be ignored
            {
                foreach (var ptc in patch)
                {
                    yield return new ChangeFile() { Path = ptc.Path, Status = ptc.Status };
                }
            }
        }

        protected static IEnumerable<string> TraverseTree(Tree tree)
        {
            foreach (TreeEntry item in tree.ToList())
            {
                if (item.TargetType == TreeEntryTargetType.Blob)
                {
                    var blob = (Blob)item.Target;
                    yield return (item.Path);
                }

                if (item.TargetType == TreeEntryTargetType.Tree)
                {
                    var subTree = (Tree)item.Target;
                    foreach (var result in TraverseTree(subTree))
                    {
                        yield return result;
                    }
                }
            }
        }

        public abstract IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);

        protected class ChangeFile
        {
            public string Path { get; set; }
            public ChangeKind Status { get; set; }
        }


    }
}
