using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
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
        private readonly ConfigHelper<string> _path;
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
            _path = new ConfigHelper<string>(path);
        }

        protected ReadWorkspace(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            _path = new ConfigHelper<string>(path);
        }
        
        /// <summary>
        /// Filters the project based on name.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the project should be included.</param>
        public ReadWorkspace WhereProject(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _whereProject;
            _whereProject = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Filters the source code file based on path.
        /// </summary>
        /// <param name="predicate">A predicate that should return <c>true</c> if the source code file should be included.</param>
        public ReadWorkspace WhereFile(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _whereFile;
            _whereFile = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }
        
        /// <summary>
        /// Filters the source code files based on extension.
        /// </summary>
        /// <param name="extensions">The extensions to include (if defined, any extensions not listed will be excluded).</param>
        public ReadWorkspace WithExtensions(params string[] extensions)
        {
            _extensions = _extensions?.Concat(extensions.Select(x => x.StartsWith(".") ? x : "." + x)).ToArray()
                ?? extensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        protected abstract IEnumerable<Project> GetProjects(string path);

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().SelectMany(input =>
            {
                string path = _path.GetValue(input, context);
                if (path != null)
                {
                    path = Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                    return GetProjects(path)
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
                                        {Keys.SourceFileRoot, Path.GetDirectoryName(document.FilePath)},
                                        {Keys.SourceFileBase, Path.GetFileNameWithoutExtension(document.FilePath)},
                                        {Keys.SourceFileExt, Path.GetExtension(document.FilePath)},
                                        {Keys.SourceFileName, Path.GetFileName(document.FilePath)},
                                        {Keys.SourceFileDir, Path.GetDirectoryName(document.FilePath)},
                                        {Keys.SourceFilePath, document.FilePath},
                                        {Keys.SourceFilePathBase, PathHelper.RemoveExtension(document.FilePath)},
                                        {Keys.RelativeFilePath, PathHelper.GetRelativePath(path, document.FilePath)},
                                        {Keys.RelativeFilePathBase, PathHelper.RemoveExtension(PathHelper.GetRelativePath(path, document.FilePath))},
                                        {Keys.RelativeFileDir, Path.GetDirectoryName(PathHelper.GetRelativePath(path, document.FilePath))}
                                    });
                                });
                        });
                }
                return (IEnumerable<IDocument>)Array.Empty<IDocument>();
            });
        }
    }
}
