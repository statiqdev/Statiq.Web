using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.GitHub
{
    public class GitHub : IModule
    {
        private readonly Credentials _credentials;
        private Uri _enterpriseUri;

        public GitHub(string username, string password)
        {
            _credentials = new Credentials(username, password);
        }

        public GitHub(string token)
        {
            _credentials = new Credentials(token);
        }

        public GitHub ConnectToEnterprise(string enterpriseUrl)
        {
            _enterpriseUri = new Uri(enterpriseUrl);
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("Wyam"), _enterpriseUri ?? GitHubClient.GitHubApiUrl)
            {
                Credentials = _credentials
            };
            return inputs;
        }
    }
}
