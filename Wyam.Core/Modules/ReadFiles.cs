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
        private readonly Func<dynamic, string> _path;
        private readonly SearchOption _searchOption;

        public ReadFiles(Func<dynamic, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            _path = path;
            _searchOption = searchOption;
        }

        // This reads all files in the path specified in Metadata.InputPath that match the specified search pattern
        public ReadFiles(string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
            : this(m => m.InputPath + searchPattern, searchOption)
        {
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            string path = Path.Combine(Environment.CurrentDirectory, _path(context.Metadata));
            foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(path), Path.GetFileName(path), _searchOption))
            {
                PipelineContext fileContext = context.Clone(file);
                fileContext.Metadata.FileRoot = path;
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
