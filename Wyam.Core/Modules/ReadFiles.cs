using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Abstractions;
using Wyam.Core.Documents;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    public class ReadFiles : IModule
    {
        private readonly Func<IDocument, string> _path;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _where = null; 

        public ReadFiles(Func<IDocument, string> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
        }

        public ReadFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _path = m => searchPattern;
        }

        public ReadFiles SearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public ReadFiles AllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public ReadFiles TopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public ReadFiles Where(Func<string, bool> predicate)
        {
            _where = predicate;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {
                string path = _path(input);
                if (path != null)
                {
                    path = Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                    path = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(path)), Path.GetFileName(path));
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        foreach (string file in Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x)))
                        {
                            string content = File.ReadAllText(file);
                            context.Trace.Verbose("Read file {0}", file);
                            yield return input.Clone(file, content, new Dictionary<string, object>
                            {
                                {MetadataKeys.SourceFileRoot, fileRoot},
                                {MetadataKeys.SourceFileBase, Path.GetFileNameWithoutExtension(file)},
                                {MetadataKeys.SourceFileExt, Path.GetExtension(file)},
                                {MetadataKeys.SourceFileName, Path.GetFileName(file)},
                                {MetadataKeys.SourceFileDir, Path.GetDirectoryName(file)},
                                {MetadataKeys.SourceFilePath, file},
                                {MetadataKeys.SourceFilePathBase, PathHelper.RemoveExtension(file)},
                                {MetadataKeys.RelativeFilePath, PathHelper.GetRelativePath(context.InputFolder, file)},
                                {MetadataKeys.RelativeFilePathBase, PathHelper.RemoveExtension(PathHelper.GetRelativePath(context.InputFolder, file))},
                                {MetadataKeys.RelativeFileDir, Path.GetDirectoryName(PathHelper.GetRelativePath(context.InputFolder, file))}
                            });
                        }
                    }
                }
            }
        }
    }
}
