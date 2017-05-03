using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor
{
    internal class HostingEnvironment : IHostingEnvironment
    {
        public HostingEnvironment(IReadOnlyFileSystem fileSystem)
        {
            EnvironmentName = "Wyam";

            // This gets used to load dependencies and is passed to Assembly.Load()
            ApplicationName = typeof(HostingEnvironment).Assembly.FullName;

            WebRootPath = fileSystem.RootPath.FullPath;
            WebRootFileProvider = new FileSystemFileProvider(fileSystem);
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