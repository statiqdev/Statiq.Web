using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Wyam.Common.Configuration;

namespace Wyam.Modules.CodeAnalysis
{
    public class ReadSolution : ReadWorkspace
    {
        public ReadSolution(string path) : base(path)
        {
        }

        public ReadSolution(DocumentConfig path) : base(path)
        {
        }

        protected override IEnumerable<Project> GetProjects(string path)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = workspace.OpenSolutionAsync(path).Result;
            return solution == null ? Array.Empty<Project>() : solution.Projects;
        }
    }
}