using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Core.Modules
{
    public class ReadFile : IModule
    {
        private readonly Func<dynamic, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFile(Func<dynamic, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            _path = path;
            _searchOption = searchOption;
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            string path = Path.Combine(Environment.CurrentDirectory, _path(context.Metadata));
            foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(path), Path.GetFileName(path), _searchOption))
            {
                PipelineContext fileContext = context.Clone(file);
                fileContext.Metadata.FileBase = Path.GetFileNameWithoutExtension(file);
                fileContext.Metadata.FileExt = Path.GetExtension(file);
                fileContext.Metadata.FileName = Path.GetFileName(file);
                fileContext.Metadata.FileDir = Path.GetDirectoryName(file);
                fileContext.Metadata.FilePath = file;
                yield return fileContext;
            }
        }

        public string Execute(PipelineContext context, string content)
        {
            return File.ReadAllText((string)context.PersistedObject);
        }
    }
}
