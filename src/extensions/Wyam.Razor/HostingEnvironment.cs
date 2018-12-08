using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor
{
    internal class HostingEnvironment : IHostingEnvironment
    {
        public HostingEnvironment(FileSystemFileProvider fileProvider)
        {
            EnvironmentName = "Wyam";

            // This gets used to load dependencies and is passed to Assembly.Load()
            ApplicationName = typeof(HostingEnvironment).Assembly.FullName;

            WebRootPath = fileProvider.WyamFileSystem.RootPath.FullPath;
            WebRootFileProvider = fileProvider;
            ContentRootPath = WebRootPath;
            ContentRootFileProvider = WebRootFileProvider;
        }

        public void ExpireChangeTokens() => ((FileSystemFileProvider)WebRootFileProvider).ExpireChangeTokens();

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}