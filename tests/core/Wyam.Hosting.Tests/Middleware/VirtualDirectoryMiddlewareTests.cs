using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using Shouldly;
using Wyam.Hosting.Tests;
using Wyam.Hosting.Middleware;

namespace Wyam.Hosting.Tests.Middleware
{
    [TestFixture]
    public class VirtualDirectoryMiddlewareTests
    {
        private static readonly Assembly TestAssembly = typeof(ScriptInjectionMiddlewareTests).Assembly;

        [TestCase("foo")]
        [TestCase("foo/")]
        [TestCase("foo/bar")]
        [TestCase("foo/bar/")]
        [TestCase("/foo")]
        [TestCase("/foo/")]
        [TestCase("/foo/bar")]
        [TestCase("/foo/bar/")]
        public async Task GetsFileUnderVirtualDirectory(string virtualDirectory)
        {
            // Given
            TestServer server = GetServer(virtualDirectory);
            if (!virtualDirectory.StartsWith("/"))
            {
                virtualDirectory = "/" + virtualDirectory;
            }
            if (!virtualDirectory.EndsWith("/"))
            {
                virtualDirectory = virtualDirectory + "/";
            }

            // When
            HttpResponseMessage response = await server.CreateRequest(virtualDirectory + "BasicHtmlDocument.html").GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            // Then
            body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile("BasicHtmlDocument.html"));
        }

        [Test]
        public async Task DoesNotGetFileNotUnderVirtualDirectory()
        {
            // Given
            TestServer server = GetServer("/foo");

            // When
            HttpResponseMessage response = await server.CreateRequest("BasicHtmlDocument.html").GetAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        }

        private TestServer GetServer(string virtualDirectory) => new TestServer(
            new WebHostBuilder()
                .Configure(builder => builder
                    .UseVirtualDirectory(virtualDirectory)
                    .UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = PathString.Empty,
                        FileProvider = new ManifestEmbeddedFileProvider(AssemblyHelper.TestAssembly, "wwwroot"),
                        ServeUnknownFileTypes = true
                    })));
    }
}
