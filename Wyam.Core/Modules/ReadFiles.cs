using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Abstractions;

namespace Wyam.Core.Modules
{
    public class ReadFiles : IModule
    {
        private readonly Func<IDocument, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFiles(Func<IDocument, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            _path = path;
            _searchOption = searchOption;
        }

        public ReadFiles(string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            _path = m => !m.Metadata.ContainsKey("InputPath") ? null : Path.Combine((string)m["InputPath"], searchPattern);
            _searchOption = searchOption;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            foreach (IDocument input in inputs)
            {
                string path = _path(input);
                if (path != null)
                {
                    path = Path.Combine(Environment.CurrentDirectory, path);
                    foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(path), Path.GetFileName(path), _searchOption))
                    {
                        string content = File.ReadAllText(file);
                        pipeline.Trace.Verbose("Read file {0}", file);
                        yield return input.Clone(content, new Dictionary<string, object>
                        {
                            {"FileRoot", Path.GetDirectoryName(path)},
                            {"FileBase", Path.GetFileNameWithoutExtension(file)},
                            {"FileExt", Path.GetExtension(file)},
                            {"FileName", Path.GetFileName(file)},
                            {"FileDir", Path.GetDirectoryName(file)},
                            {"FilePath", file}
                        });
                    }
                }
            }
        }
    }
}
