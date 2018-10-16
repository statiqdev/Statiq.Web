using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using Shouldly;
using Wyam.Hosting.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Hosting.Tests.Middleware
{
    [TestFixture]
    public class DefaultExtensionsMiddlewareTests
    {
        private static readonly Assembly TestAssembly = typeof(ScriptInjectionMiddlewareTests).Assembly;

        [Test]
        public async Task ReturnsFileWithDefaultExtension()
        {
            // Given
            TestServer server = GetServer(new DefaultExtensionsOptions());

            // When
            HttpResponseMessage response = await server.CreateClient().GetAsync("/BasicHtmlDocument");
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            body.ShouldBe(ReadFile("BasicHtmlDocument.html"));
        }

        [Test]
        public async Task ReturnsFileWithCustomExtensionWithoutDot()
        {
            // Given
            TestServer server = GetServer(new DefaultExtensionsOptions()
            {
                Extensions = new List<string> { "css" }
            });

            // When
            HttpResponseMessage response = await server.CreateClient().GetAsync("/NonHtmlDocument");
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            body.ShouldBe(ReadFile("NonHtmlDocument.css"));
        }

        [Test]
        public async Task ReturnsFileWithCustomExtensionWithDot()
        {
            // Given
            TestServer server = GetServer(new DefaultExtensionsOptions()
            {
                Extensions = new List<string> { ".css" }
            });

            // When
            HttpResponseMessage response = await server.CreateClient().GetAsync("/NonHtmlDocument");
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            body.ShouldBe(ReadFile("NonHtmlDocument.css"));
        }

        [Test]
        public async Task DoesNotReturnsFileWithNonDefaultExtension()
        {
            // Given
            TestServer server = GetServer(new DefaultExtensionsOptions());

            // When
            HttpResponseMessage response = await server.CreateClient().GetAsync("/NonHtmlDocument");
            string body = await response.Content.ReadAsStringAsync();

            // Then
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
            body.ShouldBe(string.Empty);
        }

        private TestServer GetServer(DefaultExtensionsOptions options) =>
            new TestServer(
                new WebHostBuilder()
                    .Configure(builder =>
                    {
                        IFileProvider embeddedFileProvider = new ManifestEmbeddedFileProvider(TestAssembly, "wwwroot");
                        IHostingEnvironment host = builder.ApplicationServices.GetService<IHostingEnvironment>();
                        host.WebRootFileProvider = embeddedFileProvider;
                        builder
                            .UseDefaultExtensions(options)
                            .UseStaticFiles(new StaticFileOptions
                            {
                                RequestPath = PathString.Empty,
                                FileProvider = embeddedFileProvider,
                                ServeUnknownFileTypes = true
                            });
                    }));

        private string ReadFile(string filename)
        {
            string resourceName = $"Wyam.Hosting.Tests.wwwroot.{filename}";
            using (Stream stream = TestAssembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                StreamReader reader = new StreamReader(stream);
                string fileContent = reader.ReadToEnd();
                return fileContent;
            }
        }
    }
}