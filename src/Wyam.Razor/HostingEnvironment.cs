using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Wyam.Common.Execution;
using Wyam.Razor.FileProviders;

namespace Wyam.Razor
{
    internal class HostingEnvironment : IHostingEnvironment
    {
        public HostingEnvironment(IExecutionContext context)
        {
            EnvironmentName = "Wyam";
            ApplicationName = "Wyam";
            WebRootPath = context.FileSystem.RootPath.ToString();
            WebRootFileProvider = new FileSystemFileProvider(context.FileSystem);
            ContentRootPath = WebRootPath;
            ContentRootFileProvider = WebRootFileProvider;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}