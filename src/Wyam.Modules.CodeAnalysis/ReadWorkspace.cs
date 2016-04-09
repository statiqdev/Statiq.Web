using System;
using System.Collections.Generic;
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
using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Reads an MSBuild solution or project file and returns all referenced source files as documents.
    /// This module will be executed once and input documents will be ignored if a search path is 
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input 
    /// document and the resulting output documents will be aggregated.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// See https://github.com/dotnet/roslyn/issues/212 and https://roslyn.codeplex.com/workitem/218.
    /// </summary>
    public abstract class ReadWorkspace : IModule, IAsNewDocuments
    {
        private readonly string _path;
        private readonly DocumentConfig _pathDelegate;
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
            _path = path;
        }

        protected ReadWorkspace(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            _pathDelegate = path;
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
            return _path != null
                ? Execute(null, _path, context)
                : inputs.AsParallel().SelectMany(input =>
                    Execute(input, _pathDelegate.Invoke<string>(input, context), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, string path, IExecutionContext context)
        {
            if (path != null)
            {
                path = System.IO.Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                return GetProjects(path)
                    .AsParallel()
                    .Where(project => project != null && (_whereProject == null || _whereProject(project.Name)))
                    .SelectMany(project =>
                    {
                        Trace.Verbose("Read project {0}", project.Name);
                        return project.Documents
                            .AsParallel()
                            .Where(x => !string.IsNullOrWhiteSpace(x.FilePath) && File.Exists(x.FilePath)
                                && (_whereFile == null || _whereFile(x.FilePath)) && (_extensions == null || _extensions.Contains(System.IO.Path.GetExtension(x.FilePath))))
                            .Select(document => {
                                Trace.Verbose("Read file {0}", document.FilePath);
                                return context.GetDocument(document.FilePath, File.OpenRead(document.FilePath), new Dictionary<string, object>
                                {
                                    //TODO: Change to path-based metadata
                                    {Keys.SourceFileRoot, System.IO.Path.GetDirectoryName(document.FilePath)},
                                    {Keys.SourceFileBase, System.IO.Path.GetFileNameWithoutExtension(document.FilePath)},
                                    {Keys.SourceFileExt, System.IO.Path.GetExtension(document.FilePath)},
                                    {Keys.SourceFileName, System.IO.Path.GetFileName(document.FilePath)},
                                    {Keys.SourceFileDir, System.IO.Path.GetDirectoryName(document.FilePath)},
                                    {Keys.SourceFilePath, document.FilePath},
                                    {Keys.SourceFilePathBase, PathHelper.RemoveExtension(document.FilePath)},
                                    {Keys.RelativeFilePath, PathHelper.GetRelativePath(path, document.FilePath)},
                                    {Keys.RelativeFilePathBase, PathHelper.RemoveExtension(PathHelper.GetRelativePath(path, document.FilePath))},
                                    {Keys.RelativeFileDir, System.IO.Path.GetDirectoryName(PathHelper.GetRelativePath(path, document.FilePath))}
                                });
                            });
                    });
            }
            return Array.Empty<IDocument>();
        }
    }
}
