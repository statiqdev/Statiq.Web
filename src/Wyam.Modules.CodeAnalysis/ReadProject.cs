using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Wyam.Common.Configuration;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Reads all the source files from a specified msbuild project.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// </summary>
    /// <remarks>
    /// The output of this module is similar to executing the ReadFiles module on all source files in the project.
    /// </remarks>
    /// <metadata name="SourceFileRoot" type="string">The absolute root search path without any nested directories 
    /// (I.e., the path that was searched, and possibly descended, for the given pattern).</metadata>
    /// <metadata name="SourceFilePath" type="string">The full absolute path of the file (including file name).</metadata>
    /// <metadata name="SourceFilePathBase" type="string">The full absolute path of the file (including file name) 
    /// without the file extension.</metadata>
    /// <metadata name="SourceFileBase" type="string">The file name without any extension. Equivalent 
    /// to <c>Path.GetFileNameWithoutExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileExt" type="string">The extension of the file. Equivalent 
    /// to <c>Path.GetExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileName" type="string">The full file name. Equivalent 
    /// to <c>Path.GetFileName(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileDir" type="string">The full absolute directory of the file. 
    /// Equivalent to <c>Path.GetDirectoryName(SourceFilePath).</c></metadata>
    /// <metadata name="RelativeFilePath" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder.</metadata>
    /// <metadata name="RelativeFilePathBase" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir" type="string">The relative directory of the file 
    /// from the Wyam input folder.</metadata>
    /// <category>Input/Output</category>
    public class ReadProject : ReadWorkspace
    {
        /// <summary>
        /// Reads the project file at the specified path.
        /// </summary>
        /// <param name="path">The project file path.</param>
        public ReadProject(string path) : base(path)
        {
        }

        /// <summary>
        /// Reads the project file at the specified path. This allows you to specify a different project file depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a <c>string</c> with the project file path.</param>
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