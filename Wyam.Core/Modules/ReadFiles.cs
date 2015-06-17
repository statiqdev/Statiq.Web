using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Abstractions;
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
                throw new ArgumentNullException("path");
            }

            _path = path;
        }

        public ReadFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
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
                    path = Path.Combine(context.InputFolder, path);
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        foreach (string file in Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x)))
                        {
                            string content = File.ReadAllText(file);
                            context.Trace.Verbose("Read file {0}", file);
                            yield return input.Clone(content, new Dictionary<string, object>
                            {
                                {MetadataKeys.FileRoot, fileRoot},
                                {MetadataKeys.FileBase, Path.GetFileNameWithoutExtension(file)},
                                {MetadataKeys.FileExt, Path.GetExtension(file)},
                                {MetadataKeys.FileName, Path.GetFileName(file)},
                                {MetadataKeys.FileDir, Path.GetDirectoryName(file)},
                                {MetadataKeys.FilePath, file},
                                {MetadataKeys.SourcePath, file},
                                {MetadataKeys.FileRelative, Path.Combine(PathHelper.GetRelativePath(context.InputFolder, file))}
                            });
                        }
                    }
                }
            }
        }
    }
}
