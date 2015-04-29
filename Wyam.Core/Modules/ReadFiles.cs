using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Extensibility;

namespace Wyam.Core.Modules
{
    public class ReadFiles : IModule
    {
        private readonly Func<IMetadata, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFiles(Func<IMetadata, string> path, SearchOption searchOption = SearchOption.AllDirectories)
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

            _path = m => !m.ContainsKey("InputPath") ? null : Path.Combine((string)m["InputPath"], searchPattern);
            _searchOption = searchOption;
        }

        public IEnumerable<IModuleContext> Execute(IReadOnlyList<IModuleContext> inputs, IPipelineContext pipeline)
        {
            foreach (IModuleContext input in inputs)
            {
                string path = _path(input.Metadata);
                if (path != null)
                {
                    path = Path.Combine(Environment.CurrentDirectory, path);
                    foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(path), Path.GetFileName(path), _searchOption))
                    {
                        string content = File.ReadAllText(file);
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
