using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    public class ReadFiles : IModule
    {
        private readonly Func<IMetadata, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFiles(Func<IMetadata, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (path == null) throw new ArgumentNullException("path");

            _path = path;
            _searchOption = searchOption;
        }

        public IEnumerable<IPipelineContext> Prepare(IPipelineContext context)
        {
            string path = _path(context.Metadata);
            if(path == null)
            {
                yield break;
            }
            path = Path.Combine(Environment.CurrentDirectory, path);
            foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(path), Path.GetFileName(path), _searchOption))
            {

                yield return context.Clone(file, new Dictionary<string, object>
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

        public string Execute(IPipelineContext context, string content)
        {
            if(context.PersistedObject == null)
            {
                return content;
            }
            return File.ReadAllText((string)context.PersistedObject);
        }
    }
}
