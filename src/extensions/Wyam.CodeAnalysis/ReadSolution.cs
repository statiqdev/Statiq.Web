using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Wyam.Common.Configuration;
using Wyam.Common.IO;

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
    /// <metadata name="SourceFileRoot" type="DirectoryPath">The absolute root search path without any nested directories
    /// (I.e., the path that was searched, and possibly descended, for the given pattern).</metadata>
    /// <metadata name="SourceFilePath" type="FilePath">The full absolute path of the file (including file name).</metadata>
    /// <metadata name="SourceFilePathBase" type="FilePath">The full absolute path of the file (including file name)
    /// without the file extension.</metadata>
    /// <metadata name="SourceFileBase" type="FilePath">The file name without any extension. Equivalent
    /// to <c>Path.GetFileNameWithoutExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileExt" type="string">The extension of the file. Equivalent
    /// to <c>Path.GetExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileName" type="FilePath">The full file name. Equivalent
    /// to <c>Path.GetFileName(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileDir" type="DirectoryPath">The full absolute directory of the file.
    /// Equivalent to <c>Path.GetDirectoryName(SourceFilePath).</c></metadata>
    /// <metadata name="RelativeFilePath" type="FilePath">The relative path to the file (including file name)
    /// from the Wyam input folder.</metadata>
    /// <metadata name="RelativeFilePathBase" type="FilePath">The relative path to the file (including file name)
    /// from the Wyam input folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir" type="DirectoryPath">The relative directory of the file
    /// from the Wyam input folder.</metadata>
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

        protected override IEnumerable<Project> GetProjects(IFile file)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solution = workspace.OpenSolutionAsync(file.Path.FullPath).Result;
            return solution == null ? Array.Empty<Project>() : solution.Projects;
        }
    }
}