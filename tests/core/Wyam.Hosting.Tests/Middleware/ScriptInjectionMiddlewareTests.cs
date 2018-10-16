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
    public class ScriptInjectionMiddlewareTests
    {
        [TestCase("BasicHtmlDocument.html", true)]
        [TestCase("BasicHtmlDocumentNoBodyEnd.html", false)]
        [TestCase("NonHtmlDocument.css", false)]
        public async Task InjectScriptWhenAppropriate(string filename, bool inject)
        {
            // Given
            TestServer server = GetServer();

            // When
            HttpResponseMessage response = await server.CreateRequest(filename).GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            if (inject)
            {
                body.ShouldContain("<script type=\"text/javascript\" src=\"/livereload.js\"></script></body>");
            }
            else
            {
                body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile(filename));
            }
        }

        private TestServer GetServer() => new TestServer(
            new WebHostBuilder()
                .Configure(builder => builder
                    .UseScriptInjection("/livereload.js")
                    .UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = PathString.Empty,
                        FileProvider = new ManifestEmbeddedFileProvider(AssemblyHelper.TestAssembly, "wwwroot"),
                        ServeUnknownFileTypes = true
                    })));
    }
}