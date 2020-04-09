using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Statiq.Common;

namespace Statiq.Web.GitHub
{
    /// <summary>
    /// Deploys output files to GitHub Pages.
    /// </summary>
    /// <category>Deployment</category>
    public class DeployGitHubPages : MultiConfigModule
    {
        private const string Owner = nameof(Owner);
        private const string Name = nameof(Name);
        private const string Username = nameof(Username);
        private const string Password = nameof(Password);
        private const string Token = nameof(Token);
        private const string Branch = nameof(Branch);
        private const string SourcePath = nameof(SourcePath);

        /// <summary>
        /// Deploys the current output folder to GitHub Pages.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The name of the repository.</param>
        /// <param name="username">A GitHub username to authenticate with.</param>
        /// <param name="password">A GitHub password to authenticate with.</param>
        public DeployGitHubPages(Config<string> owner, Config<string> name, Config<string> username, Config<string> password)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Owner, owner },
                    { Name, name },
                    { Username, username },
                    { Password, password }
                },
                false)
        {
        }

        /// <summary>
        /// Deploys the current output folder to GitHub Pages.
        /// </summary>
        /// <param name="owner">The repository owner.</param>
        /// <param name="name">The name of the repository.</param>
        /// <param name="token">A GitHub access token to authenticate with.</param>
        public DeployGitHubPages(Config<string> owner, Config<string> name, Config<string> token)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Owner, owner },
                    { Name, name },
                    { Token, token }
                },
                false)
        {
        }

        /// <summary>
        /// Specifies the branch to push output files to (defaults to "gh-pages").
        /// </summary>
        /// <param name="branch">The branch to push changes to.</param>
        /// <returns>The current module instance.</returns>
        public DeployGitHubPages ToBranch(Config<string> branch) => (DeployGitHubPages)SetConfig(Branch, branch);

        /// <summary>
        /// The folder to deploy (defaults to the current output folder).
        /// </summary>
        /// <param name="path">The path of the folder to deploy.</param>
        /// <returns>The current module instance.</returns>
        public DeployGitHubPages FromPath(Config<NormalizedPath> path) => (DeployGitHubPages)SetConfig(SourcePath, path);

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(
            IDocument input,
            IExecutionContext context,
            IMetadata values)
        {
            // See http://www.levibotelho.com/development/commit-a-file-with-the-github-api/

            // Set up the client
            GitHubClient github = new GitHubClient(new ProductHeaderValue("Statiq"), GitHubClient.GitHubApiUrl);
            if (values.TryGetValue(Token, out string token))
            {
                github.Credentials = new Credentials(token);
            }
            else if (values.TryGetValue(Username, out string username) && values.TryGetValue(Password, out string password))
            {
                github.Credentials = new Credentials(username, password);
            }
            else
            {
                throw new ExecutionException("Could not determine GitHub credentials");
            }
            if (!values.TryGetValue(Owner, out string owner)
                || string.IsNullOrEmpty(owner)
                || !values.TryGetValue(Name, out string name)
                || string.IsNullOrEmpty(name))
            {
                throw new ExecutionException("Invalid repository owner or name");
            }

            // Get the current head tree
            string branch = values.GetString(Branch, "gh-pages");
            Reference reference = await github.Git.Reference.Get(owner, name, "heads/" + branch);
            Commit commit = await github.Git.Commit.Get(owner, name, reference.Object.Sha);

            // Iterate the output path, adding new tree items
            // Don't reference a base tree so that any items not reflected in the new tree will be deleted
            NormalizedPath sourcePath = values.GetPath(SourcePath, context.FileSystem.GetOutputPath());
            NewTree newTree = new NewTree();
            foreach (string outputFile in Directory.GetFiles(sourcePath.FullPath, "*", SearchOption.AllDirectories))
            {
                // Upload the blob
                BlobReference blob = await github.Git.Blob.Create(owner, name, new NewBlob
                {
                    Content = Convert.ToBase64String(await File.ReadAllBytesAsync(outputFile)),
                    Encoding = EncodingType.Base64
                });

                // Add the new blob to the tree
                string relativePath = Path.GetRelativePath(sourcePath.FullPath, outputFile).Replace("\\", "/");
                newTree.Tree.Add(new NewTreeItem
                {
                    Path = relativePath,
                    Mode = "100644",
                    Type = TreeType.Blob,
                    Sha = blob.Sha
                });
            }

            // Create the new tree
            TreeResponse newTreeResponse = await github.Git.Tree.Create(owner, name, newTree);

            // Create the commit
            Commit newCommit = await github.Git.Commit.Create(owner, name, new NewCommit($"Deployment from Statiq", newTreeResponse.Sha, commit.Sha));

            // Update the head ref
            await github.Git.Reference.Update(owner, name, reference.Ref, new ReferenceUpdate(newCommit.Sha, true));

            return await input.YieldAsync();
        }
    }
}
