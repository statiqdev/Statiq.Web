using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Wyam.Modules.CodeAnalysis
{
    public class ReadProject : ReadWorkspace
    {
        public ReadProject(string path) : base(path)
        {
        }

        protected override IEnumerable<Project> GetProjects()
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = workspace.OpenProjectAsync(WorkspacePath).Result;
            return new[] {project};
        }
    }
}