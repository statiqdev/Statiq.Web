using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Modules;

namespace Wyam.Core
{
    public static class ReadFilesExtensions
    {
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

            return builder.AddModule(new ReadFiles(m => m.InputPath == null ? null : Path.Combine(m.InputPath, searchPattern), searchOption));
        }
    }
}
