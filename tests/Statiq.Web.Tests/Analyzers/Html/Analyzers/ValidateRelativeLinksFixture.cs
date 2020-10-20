using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Web.Tests.Analyzers.Html.Analyzers
{
    [TestFixture]
    public class ValidateRelativeLinksFixture : BaseFixture
    {
        public class AnalyzeTests : ValidateRelativeLinksFixture
        {
            [TestCase("../target.html", true)]
            [TestCase("../target.html?query=parameter", true)]
            [TestCase("../target.html#fragment", true)]
            [TestCase("../target.html?query=parameter#fragment", true)]
            [TestCase("../foobar.html", false)]
            public async Task CanValidateAscendingRelativeLink(string link, bool success)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/target.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/test/document.html"),
                    new NormalizedPath("test/document.html"),
                    $"<html><head></head><body><a href=\"{link}\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document)
                {
                    FileSystem = fileSystem
                };
                ValidateRelativeLinks validateRelativeLinks = new ValidateRelativeLinks();

                // When
                await validateRelativeLinks.AnalyzeAsync(context);

                // Then
                if (success)
                {
                    context.AnalyzerResults.ShouldBeEmpty();
                }
                else
                {
                    context.AnalyzerResults.ShouldHaveSingleItem();
                }
            }

            [TestCase("nested", "/document.html", true)]
            [TestCase("foobar", "/document.html", false)]
            [TestCase("/nested", "/document.html", true)]
            [TestCase("/foobar", "/document.html", false)]
            [TestCase("nested/", "/document.html", true)]
            [TestCase("foobar/", "/document.html", false)]
            [TestCase("nested", "/test/document.html", false)]
            [TestCase("foobar", "/test/document.html", false)]
            [TestCase("/nested", "/test/document.html", false)]
            [TestCase("/foobar", "/test/document.html", false)]
            [TestCase("nested/", "/test/document.html", false)]
            [TestCase("foobar/", "/test/document.html", false)]
            public async Task CanValidateRelativeLinkWithBaseUri(string baseHref, string source, bool success)
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
                    new NormalizedPath(source),
                    new NormalizedPath(source.TrimStart('/')),
                    $"<html><head><base href=\"{baseHref}\" /></head><body><a href=\"target.html\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document)
                {
                    FileSystem = fileSystem
                };
                ValidateRelativeLinks validateRelativeLinks = new ValidateRelativeLinks();

                // When
                await validateRelativeLinks.AnalyzeAsync(context);

                // Then
                if (success)
                {
                    context.AnalyzerResults.ShouldBeEmpty();
                }
                else
                {
                    context.AnalyzerResults.ShouldHaveSingleItem();
                }
            }

            [TestCase("targët")]
            [TestCase("targ%C3%ABt")]
            public async Task HandlesSpecialCharacters(string link)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/targët.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/document.html"),
                    new NormalizedPath("document.html"),
                    $"<html><head></head><body><a href=\"{link}\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document)
                {
                    FileSystem = fileSystem
                };
                ValidateRelativeLinks validateRelativeLinks = new ValidateRelativeLinks();

                // When
                await validateRelativeLinks.AnalyzeAsync(context);

                // Then
                context.AnalyzerResults.ShouldBeEmpty();
            }
        }
    }
}
