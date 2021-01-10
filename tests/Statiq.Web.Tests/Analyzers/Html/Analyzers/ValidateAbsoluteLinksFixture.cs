using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Markdig.Helpers;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Web.Tests.Analyzers.Html.Analyzers
{
    [TestFixture]
    public class ValidateAbsoluteLinksFixture : BaseFixture
    {
        public class AnalyzeTests : ValidateAbsoluteLinksFixture
        {
            [Test]
            public async Task DoesNotThrowForInvalidLink()
            {
                // Given
                TestDocument document = new TestDocument(
                    "<html><head></head><body><a href=\"http://example.com<>\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document);
                ValidateAbsoluteLinks validateAbsoluteLinks = new ValidateAbsoluteLinks();

                // When
                await validateAbsoluteLinks.AnalyzeAsync(context);

                // Then
                context.AnalyzerResults.ShouldHaveSingleItem();
            }

            [TestCase(true)]
            [TestCase(false)]
            public async Task ValidatesAbsoluteLink(bool success)
            {
                // Given
                TestDocument document = new TestDocument(
                    $"<html><head></head><body><a href=\"https://statiq.dev\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document);
                context.HttpResponseFunc = (_, __) => new HttpResponseMessage
                    {
                        StatusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                        Content = new System.Net.Http.StringContent(string.Empty)
                    };
                ValidateAbsoluteLinks validateAbsoluteLinks = new ValidateAbsoluteLinks();

                // When
                await validateAbsoluteLinks.AnalyzeAsync(context);

                // Then
                if (success)
                {
                    context.AnalyzerResults.ShouldBeEmpty();
                }
                else
                {
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [Test]
            public async Task CanValidateRelativeLinkWithAbsoluteBaseUri()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddDirectory("/output/nested");
                fileProvider.AddFile("/output/nested/target.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/test/document.html"),
                    new NormalizedPath("test/document.html"),
                    $"<html><head><base href=\"https://statiq.dev\" /></head><body><a href=\"page\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document)
                {
                    FileSystem = fileSystem
                };
                string requestUri = null;
                context.HttpResponseFunc = (request, __) =>
                {
                    requestUri = request.RequestUri.ToString();
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new System.Net.Http.StringContent(string.Empty)
                    };
                };
                ValidateAbsoluteLinks validateAbsoluteLinks = new ValidateAbsoluteLinks();

                // When
                await validateAbsoluteLinks.AnalyzeAsync(context);

                // Then
                requestUri.ShouldBe("https://statiq.dev/page");
            }
        }
    }
}
