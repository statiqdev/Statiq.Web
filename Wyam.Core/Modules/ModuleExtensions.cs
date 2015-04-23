using System;
using System.Collections.Generic;
using System.IO;

namespace Wyam.Core.Modules
{
    public static class AppendPipelineExtensions
    {
        public static IPipeline Append(this IPipeline pipeline, string content)
        {
            return pipeline.AddModule(new Append(content));
        }
    }
}
namespace Wyam.Core.Modules
{
    public static class ContentPipelineExtensions
    {
        public static IPipeline Content(this IPipeline pipeline, string content)
        {
            return pipeline.AddModule(new Content(content));
        }
    }
}
namespace Wyam.Core.Modules
{
    public static class DelegatesPipelineExtensions
    {
        public static IPipeline Delegates(this IPipeline pipeline, Func<IPipelineContext, IEnumerable<IPipelineContext>> prepare, Func<IPipelineContext, string, string> execute)
        {
            return pipeline.AddModule(new Delegates(prepare, execute));
        }
    }
}
namespace Wyam.Core.Modules
{
    public static class PrependPipelineExtensions
    {
        public static IPipeline Prepend(this IPipeline pipeline, string content)
        {
            return pipeline.AddModule(new Prepend(content));
        }
    }
}
namespace Wyam.Core.Modules
{
    public static class ReadFilesPipelineExtensions
    {
        public static IPipeline ReadFiles(this IPipeline pipeline, Func<IMetadata, string> path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return pipeline.AddModule(new ReadFiles(path, searchOption));
        }
        public static IPipeline ReadFiles(this IPipeline pipeline, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return pipeline.AddModule(new ReadFiles(searchPattern, searchOption));
        }
    }
}
namespace Wyam.Core.Modules
{
    public static class WriteFilesPipelineExtensions
    {
        public static IPipeline WriteFiles(this IPipeline pipeline, Func<IMetadata, string> path)
        {
            return pipeline.AddModule(new WriteFiles(path));
        }
        public static IPipeline WriteFiles(this IPipeline pipeline, string extension)
        {
            return pipeline.AddModule(new WriteFiles(extension));
        }
    }
}
