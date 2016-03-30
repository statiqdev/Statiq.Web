using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Modules;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Git
{
    public abstract class GitModule : IModule
    {
        private DirectoryPath _repositoryPath;
        private bool _validRepositoryPath;

        protected GitModule()
        {
        }

        protected GitModule(DirectoryPath repositoryPath)
        {
            if (repositoryPath != null && !repositoryPath.IsAbsolute)
            {
                throw new ArgumentException("The repository location must be absolute", nameof(repositoryPath));
            }
            _repositoryPath = repositoryPath;
        }

        // Returns the absolute path of a valid repository
        protected DirectoryPath GetRepositoryPath(IExecutionContext context)
        {
            // If we've already checked, return the valid one
            if (_validRepositoryPath)
            {
                return _repositoryPath;
            }

            // Get candidate paths
            List<DirectoryPath> candidatePaths = new List<DirectoryPath>();
            if (_repositoryPath != null)
            {
                candidatePaths.Add(_repositoryPath);
            }
            else
            {
                candidatePaths.AddRange(context.FileSystem.InputPaths
                    .Reverse()
                    .Select(x => context.FileSystem.RootPath.Combine(x)));
                candidatePaths.Add(context.FileSystem.RootPath);
            }

            // Find the candidate (or one of it's roots) that contains a repo
            foreach (DirectoryPath candidatePath in candidatePaths)
            {
                DirectoryPath testPath = candidatePath;
                while (testPath != null && !Repository.IsValid(testPath.FullPath))
                {
                    testPath = testPath.Parent;
                }
                if (testPath != null)
                {
                    _repositoryPath = testPath;
                    return testPath;
                }
            }

            // If we got here, we didn't get a valid path
            throw new InvalidOperationException("No repository could be found");
        }

        protected ImmutableArray<IDocument> GetCommitDocuments(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            using (Repository repository = new Repository(GetRepositoryPath(context).FullPath))
            {
                return repository.Commits
                    .OrderByDescending(x => x.Author.When)
                    .Select(x => context.GetDocument(new MetadataItems
                    {
                        new MetadataItem(GitKeys.Sha, x.Sha),
                        new MetadataItem(GitKeys.Parents, x.Parents.Select(y => y.Sha).ToImmutableArray()),
                        new MetadataItem(GitKeys.AuthorName, x.Author.Name),
                        new MetadataItem(GitKeys.AuthorEmail, x.Author.Email),
                        new MetadataItem(GitKeys.AuthorWhen, x.Author.When),
                        new MetadataItem(GitKeys.CommitterName, x.Committer.Name),
                        new MetadataItem(GitKeys.CommitterEmail, x.Committer.Email),
                        new MetadataItem(GitKeys.CommitterWhen, x.Committer.When),
                        new MetadataItem(GitKeys.Message, x.Message),
                        new MetadataItem(GitKeys.Entries,
                            CompareTrees(repository, x).ToImmutableDictionary(y => new FilePath(y.Path), y => y.Status.ToString()))
                    }))
                    .ToImmutableArray();
            }
        }

        private static IEnumerable<Entry> CompareTrees(Repository repo, Commit toCheck)
        {
            Tree commitTree = toCheck.Tree; // Main Tree
            List<Tree> parentCommitTrees = toCheck.Parents.Select(x => x.Tree).ToList(); // Parent Tree

            IEnumerable<PatchEntryChanges> patch = parentCommitTrees.Select(x => repo.Diff.Compare<Patch>(x, commitTree)).SelectMany(x => x); // Difference

            if (!parentCommitTrees.Any()) // Don't use Patch for commits without parents.
            {
                foreach (string ptc in TraverseTree(commitTree))
                {
                    yield return new Entry { Path = ptc, Status = ChangeKind.Added };
                }
            }
            else if (!parentCommitTrees.Skip(1).Any()) // Merge doesn't count to History. So everything that has more than one parent will be ignored
            {
                foreach (PatchEntryChanges ptc in patch)
                {
                    yield return new Entry { Path = ptc.Path, Status = ptc.Status };
                }
            }
        }

        private static IEnumerable<string> TraverseTree(Tree tree)
        {
            foreach (TreeEntry item in tree.ToList())
            {
                if (item.TargetType == TreeEntryTargetType.Blob)
                {
                    Blob blob = (Blob)item.Target;
                    yield return (item.Path);
                }

                if (item.TargetType == TreeEntryTargetType.Tree)
                {
                    Tree subTree = (Tree)item.Target;
                    foreach (string result in TraverseTree(subTree))
                    {
                        yield return result;
                    }
                }
            }
        }

        private class Entry
        {
            public string Path { get; set; }
            public ChangeKind Status { get; set; }
        }

        public abstract IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);
    }
}
