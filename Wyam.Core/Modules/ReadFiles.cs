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
                    string directory = Path.GetDirectoryName(path);
                    if(directory != null && Directory.Exists(directory))
                    {
                        foreach (string file in Directory.EnumerateFiles(directory, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x)))
                        {
                            string content = File.ReadAllText(file);
                            context.Trace.Verbose("Read file {0}", file);
                            string fileRoot = Path.GetDirectoryName(path);
                            string fileDir = Path.GetDirectoryName(file);
                            yield return input.Clone(content, new Dictionary<string, object>
                            {
                                {"FileRoot", fileRoot},
                                {"FileBase", Path.GetFileNameWithoutExtension(file)},
                                {"FileExt", Path.GetExtension(file)},
                                {"FileName", Path.GetFileName(file)},
                                {"FileDir", fileDir},
                                {"FilePath", file},
                                {"FileRelative", Path.Combine(PathHelper.GetRelativePath(context.InputFolder, file))}
                            });
                        }
                    }
                }
            }
        }
    }
}
