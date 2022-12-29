using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using Shouldly;
using Statiq.Web.Hosting.Middleware;

namespace Statiq.Web.Hosting.Tests.Middleware
{
    [TestFixture]
    public class ScriptInjectionMiddlewareTests
    {
        [TestCase("BasicHtmlDocument.html", "<script type=\"text/javascript\" src=\"/livereload.js\"></script></body>", 133)]
        [TestCase("BasicHtmlDocumentNoBody.html", "<script type=\"text/javascript\" src=\"/livereload.js\"></script></html>", 130)]
        [TestCase("BasicHtmlDocumentNoHtml.html", "<script type=\"text/javascript\" src=\"/livereload.js\"></script>", 77)]
        public async Task ShouldInjectScriptAtCorrectLocation(string filename, string injected, int injectionPosition)
        {
            // Given
            TestServer server = GetServer();

            // When
            HttpResponseMessage response = await server.CreateRequest(filename).GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            body.Replace("\r", string.Empty).Replace("\n", string.Empty).LastIndexOf(injected).ShouldBe(injectionPosition);
        }

        [Test]
        public async Task ShouldNotInjectNonHtmlContent()
        {
            // Given
            TestServer server = GetServer();

            // When
            HttpResponseMessage response = await server.CreateRequest("NonHtmlDocument.css").GetAsync();
            string body = await response.Content.ReadAsStringAsync();

            // Then
            body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile("NonHtmlDocument.css"));
        }

        private TestServer GetServer() => new TestServer(
            new WebHostBuilder()
                .Configure(app => app
                    .UseScriptInjection("/livereload.js")
                    .UseStaticFiles(new StaticFileOptions
                    {
                        RequestPath = PathString.Empty,
                        FileProvider = new ManifestEmbeddedFileProvider(AssemblyHelper.TestAssembly, "wwwroot"),
                        ServeUnknownFileTypes = true
                    })));
    }
}