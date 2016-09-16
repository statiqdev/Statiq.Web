using System.Reflection;
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

            // This gets used to load dependencies and is passed to Assembly.Load()
            ApplicationName = typeof(HostingEnvironment).Assembly.FullName;

            WebRootPath = context.FileSystem.RootPath.FullPath;
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