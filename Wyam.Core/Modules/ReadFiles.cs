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
                IPipelineContext fileContext = context.Clone(file);
                fileContext.Metadata.FileRoot = path;
                fileContext.Metadata.FileBase = Path.GetFileNameWithoutExtension(file);
                fileContext.Metadata.FileExt = Path.GetExtension(file);
                fileContext.Metadata.FileName = Path.GetFileName(file);
                fileContext.Metadata.FileDir = Path.GetDirectoryName(file);
                fileContext.Metadata.FilePath = file;
                yield return fileContext;
            }
        }

        public string Execute(IPipelineContext context, string content)
        {
            if(context.ExecutionObject == null)
            {
                return content;
            }
            return File.ReadAllText((string)context.ExecutionObject);
        }
    }
}
