using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Helpers;
using Wyam.Core.Modules;

namespace Wyam.Core
{
    public static class WriteFilesExtensions
    {
        public static IPipelineBuilder WriteFiles(this IPipelineBuilder builder,
            Func<dynamic, string> path)
        {
            return builder.AddModule(new WriteFiles(path));
        }
        
        // This writes a file per content to the path specified in Metadata.OutputPath with the same relative path as the input file and with the specified extension
        public static IPipelineBuilder WriteFiles(this IPipelineBuilder builder,
            string extension)
        {
            if (extension == null) throw new ArgumentNullException("extension");

            return builder.AddModule(new WriteFiles(m => 
                (m.OutputPath == null || m.FileRoot == null || m.FilePath == null || m.FileBase == null) ? null :
                Path.Combine(m.OutputPath, PathHelper.GetRelativePath(m.FileRoot, m.FilePath), m.FileBase, 
                    (extension.StartsWith(".") ? extension : ("." + extension)))));
        }
    }
}
