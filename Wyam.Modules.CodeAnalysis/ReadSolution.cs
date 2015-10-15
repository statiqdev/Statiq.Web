using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Wyam.Modules.CodeAnalysis
{
    public class ReadSolution : ReadWorkspace
    {
        public ReadSolution(string path) : base(path)
        {
        }

        protected override IEnumerable<Project> GetProjects()
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = workspace.OpenSolutionAsync(WorkspacePath).Result;
            return solution == null ? Array.Empty<Project>() : solution.Projects;
        }
    }
}