using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Wyam.Common.Configuration;

namespace Wyam.Modules.CodeAnalysis
{
    public class ReadProject : ReadWorkspace
    {
        public ReadProject(string path) : base(path)
        {
        }

        public ReadProject(DocumentConfig path) : base(path)
        {
        }

        protected override IEnumerable<Project> GetProjects(string path)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = workspace.OpenProjectAsync(path).Result;
            return new[] {project};
        }
    }
}