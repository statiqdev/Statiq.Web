using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    public class WriteFiles : IModule
    {
        private readonly Func<dynamic, string> _path;

        public WriteFiles(Func<dynamic, string> path)
        {
            _path = path;
        }

        // This writes a file per content to the path specified in Metadata.OutputPath with the same relative path as the input file and with the specified extension
        public WriteFiles(string extension)
            : this(m => Path.Combine(m.OutputPath, PathHelper.GetRelativePath(m.FileRoot, m.FilePath)) + m.FileBase + (extension.StartsWith(".") ? extension : ("." + extension)))
        {
        }

        public IEnumerable<PipelineContext> Prepare(PipelineContext context)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public string Execute(PipelineContext context, string content)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
