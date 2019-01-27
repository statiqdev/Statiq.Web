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
            body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile("BasicHtmlDocument.html"));
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
            body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile("NonHtmlDocument.css"));
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
            body.ShouldBe(AssemblyHelper.ReadEmbeddedWebFile("NonHtmlDocument.css"));
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
                    .Configure(app =>
                    {
                        IFileProvider embeddedFileProvider = new ManifestEmbeddedFileProvider(AssemblyHelper.TestAssembly, "wwwroot");
                        IHostingEnvironment host = app.ApplicationServices.GetService<IHostingEnvironment>();
                        host.WebRootFileProvider = embeddedFileProvider;
                        app
                            .UseDefaultExtensions(options)
                            .UseStaticFiles(new StaticFileOptions
                            {
                                RequestPath = PathString.Empty,
                                FileProvider = embeddedFileProvider,
                                ServeUnknownFileTypes = true
                            });
                    }));
    }
}