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
    public static class PipelineBuilderExtensions
    {
        // Execute

        public static IPipelineBuilder Execute(this IPipelineBuilder builder, 
            Func<IPipelineContext, IEnumerable<IPipelineContext>> prepare, 
            Func<IPipelineContext, string, string> execute)
        {
            return builder.AddModule(new Execute(prepare, execute));
        }

        // ReadFiles

        public static IPipelineBuilder ReadFiles(this IPipelineBuilder builder, 
            Func<dynamic, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return builder.AddModule(new ReadFiles(path, searchOption));
        }

        // This reads all files in the path specified in Metadata.InputPath that match the specified search pattern
        public static IPipelineBuilder ReadFiles(this IPipelineBuilder builder,
            string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (searchPattern == null) throw new ArgumentNullException("searchPattern");

            return builder.AddModule(new ReadFiles(m => m.InputPath == null ? null : (m.InputPath + searchPattern), searchOption));
        }

        // WriteFiles

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
                (Path.Combine(m.OutputPath, PathHelper.GetRelativePath(m.FileRoot, m.FilePath)) 
                + m.FileBase 
                + (extension.StartsWith(".") ? extension : ("." + extension)))));
        }
    }
}
