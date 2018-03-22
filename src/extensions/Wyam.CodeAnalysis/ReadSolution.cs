using System.Collections.Generic;
using System.IO;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Reads all the source files from a specified msbuild solution.
    /// This module will be executed once and input documents will be ignored if a search path is
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input
    /// document and the resulting output documents will be aggregated.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// </summary>
    /// <remarks>
    /// The output of this module is similar to executing the ReadFiles module on all source files in the solution.
    /// </remarks>
    /// <metadata cref="CodeAnalysisKeys.AssemblyName" usage="Output" />
    /// <metadata cref="Keys.SourceFileRoot" usage="Output" />
    /// <metadata cref="Keys.SourceFileBase" usage="Output" />
    /// <metadata cref="Keys.SourceFileExt" usage="Output" />
    /// <metadata cref="Keys.SourceFileName" usage="Output" />
    /// <metadata cref="Keys.SourceFileDir" usage="Output" />
    /// <metadata cref="Keys.SourceFilePath" usage="Output" />
    /// <metadata cref="Keys.SourceFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePath" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFileDir" usage="Output" />
    /// <category>Input/Output</category>
    public class ReadSolution : ReadWorkspace
    {
        /// <summary>
        /// Reads the solution file at the specified path.
        /// </summary>
        /// <param name="path">The solution file path.</param>
        public ReadSolution(FilePath path)
            : base(path)
        {
        }

        /// <summary>
        /// Reads the solution file at the specified path. This allows you to specify a different solution file depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a <c>FilePath</c> with the solution file path.</param>
        public ReadSolution(DocumentConfig path)
            : base(path)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<Project> GetProjects(IFile file)
        {
            StringWriter log = new StringWriter();
            AnalyzerManager manager = new AnalyzerManager(file.Path.Directory.FullPath, new AnalyzerManagerOptions
            {
                LogWriter = log
            });
            foreach (ProjectAnalyzer analyzer in manager.Projects.Values)
            {
                CompileProjectAndTrace(analyzer, log);
            }
            Workspace workspace = manager.GetWorkspace();
            return workspace.CurrentSolution.Projects;
        }
    }
}