using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Modules.Basic
{
    public class ReadFile : IModule
    {
        private readonly Func<Metadata, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFile(Func<Metadata, string> path, SearchOption searchOption = SearchOption.AllDirectories)
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
                fileContext.Metadata.Set("FileBase", Path.GetFileNameWithoutExtension(file));
                fileContext.Metadata.Set("FileExt", Path.GetExtension(file));
                fileContext.Metadata.Set("FileName", Path.GetFileName(file));
                fileContext.Metadata.Set("FileDir", Path.GetDirectoryName(file));
                fileContext.Metadata.Set("FilePath", file);
                yield return fileContext;
            }
        }

        public string Execute(PipelineContext context, string content)
        {
            return File.ReadAllText((string)context.PersistedObject);
        }
    }
}
