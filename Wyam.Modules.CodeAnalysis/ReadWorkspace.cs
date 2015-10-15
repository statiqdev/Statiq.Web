using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Reads an MSBuild solution or project file and returns all referenced source files as documents.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// See https://github.com/dotnet/roslyn/issues/212 and https://roslyn.codeplex.com/workitem/218.
    /// </summary>
    public abstract class ReadWorkspace : IModule
    {
        private Func<string, bool> _whereProject;
        private Func<string, bool> _whereFile;
        private string[] _extensions;

        protected ReadWorkspace(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(nameof(path));
            }
            WorkspacePath = path;
        }

        // Filters the projects based on name
        public ReadWorkspace WhereProject(Func<string, bool> predicate)
        {
            _whereProject = predicate;
            return this;
        }

        // Filters the files based on path
        public ReadWorkspace WhereFile(Func<string, bool> predicate)
        {
            _whereFile = predicate;
            return this;
        }

        // Filters the files based on extension
        public ReadWorkspace WithExtensions(params string[] extensions)
        {
            _extensions = extensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        protected string WorkspacePath { get; }

        protected abstract IEnumerable<Project> GetProjects();

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return GetProjects()
                .AsParallel()
                .Where(project => project != null && (_whereProject == null || _whereProject(project.Name)))
                .SelectMany(project =>
                {
                    context.Trace.Verbose("Read project {0}", project.Name);
                    return project.Documents
                        .AsParallel()
                        .Where(x => !string.IsNullOrWhiteSpace(x.FilePath) && File.Exists(x.FilePath)
                            && (_whereFile == null || _whereFile(x.FilePath)) && (_extensions == null || _extensions.Contains(Path.GetExtension(x.FilePath))))
                        .Select(document => {
                            context.Trace.Verbose("Read file {0}", document.FilePath);
                            return context.GetNewDocument(document.FilePath, File.OpenRead(document.FilePath), new Dictionary<string, object>
                            {
                                {"SourceFileRoot", Path.GetDirectoryName(document.FilePath)},
                                {"SourceFileBase", Path.GetFileNameWithoutExtension(document.FilePath)},
                                {"SourceFileExt", Path.GetExtension(document.FilePath)},
                                {"SourceFileName", Path.GetFileName(document.FilePath)},
                                {"SourceFileDir", Path.GetDirectoryName(document.FilePath)},
                                {"SourceFilePath", document.FilePath},
                                {"SourceFilePathBase", PathHelper.RemoveExtension(document.FilePath)},
                                {"RelativeFilePath", PathHelper.GetRelativePath(WorkspacePath, document.FilePath)},
                                {"RelativeFilePathBase", PathHelper.RemoveExtension(PathHelper.GetRelativePath(WorkspacePath, document.FilePath))},
                                {"RelativeFileDir", Path.GetDirectoryName(PathHelper.GetRelativePath(WorkspacePath, document.FilePath))}
                            });
                        });
                });
        }
    }
}
