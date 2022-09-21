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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("/link-root/foo/target.html", true)]
            [TestCase("link-root/foo/target.html", false)]
            [TestCase("/link-root/target.html", false)]
            [TestCase("link-root/target.html", false)]
            [TestCase("../link-root/foo/target.html", false)]
            [TestCase("../../link-root/foo/target.html", true)]
            [TestCase("../foo/target.html", true)]
            [TestCase("../target.html", false)]
            public async Task CanValidateRelativeLinkWithLinkRoot(string link, bool success)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/foo/target.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
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
                context.Settings.Add(Keys.LinkRoot, "link-root");
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("/foo/target", true)]
            [TestCase("foo/target", false)]
            [TestCase("../foo/target", true)]
            [TestCase("/target", false)]
            [TestCase("target", false)]
            [TestCase("../target", false)]
            public async Task CanValidateExtensionlessLinks(string link, bool success)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/foo/target.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("/foo", true)]
            [TestCase("foo", false)]
            [TestCase("../foo", true)]
            [TestCase("/", true)]
            [TestCase("/foo.html", false)]
            [TestCase("/foo/index", true)]
            [TestCase("foo/index", false)]
            [TestCase("../foo/index", true)]
            [TestCase("/index", false)]
            [TestCase("index", false)]
            [TestCase("../index", false)]
            [TestCase("/foo/index.html", true)]
            [TestCase("foo/index.html", false)]
            [TestCase("../foo/index.html", true)]
            [TestCase("/index.html", false)]
            [TestCase("index.html", false)]
            [TestCase("../index.html", false)]
            public async Task CanValidateIndexLinks(string link, bool success)
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/foo/index.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("nested", "/document.html", "target.html", true)]
            [TestCase("foobar", "/document.html", "target.html", false)]
            [TestCase("/nested", "/document.html", "target.html", true)]
            [TestCase("/foobar", "/document.html", "target.html", false)]
            [TestCase("/nested/", "/document.html", "target.html", true)]
            [TestCase("/foobar/", "/document.html", "target.html", false)]
            [TestCase("nested/", "/document.html", "target.html", true)]
            [TestCase("foobar/", "/document.html", "target.html", false)]
            [TestCase("nested", "/test/document.html", "target.html", false)]
            [TestCase("foobar", "/test/document.html", "target.html", false)]
            [TestCase("/nested", "/test/document.html", "target.html", true)]
            [TestCase("/foobar", "/test/document.html", "target.html", false)]
            [TestCase("/nested/", "/test/document.html", "target.html", true)]
            [TestCase("/foobar/", "/test/document.html", "target.html", false)]
            [TestCase("nested/", "/test/document.html", "target.html", false)]
            [TestCase("foobar/", "/test/document.html", "target.html", false)]
            [TestCase("/", "/test/document.html", "nested/target.html", true)]
            [TestCase("", "/test/document.html", "nested/target.html", false)]
            public async Task CanValidateRelativeLinkWithBaseUri(string baseHref, string source, string link, bool success)
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
                    $"<html><head><base href=\"{baseHref}\" /></head><body><a href=\"{link}\">foo</a></body></html>",
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
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

            [Test]
            public async Task HandlesEscapedUnicode()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/output");
                fileProvider.AddFile("/output/ไทย.html", "<html><head></head><body><h1>Hello World!</h1></body></html>");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/document.html"),
                    new NormalizedPath("document.html"),
                    $"<html><head></head><body><a href=\"&#xE44;&#xE17;&#xE22;\">foo</a></body></html>",
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

            [TestCase("#", "/document.html", true)]
            [TestCase("#foo", "/document.html", true)]
            [TestCase("target#foo", "/document.html", true)]
            [TestCase("target2#foo", "/document.html", false)] // Sanity check
            [TestCase("#", "/foo/document.html", true)]
            [TestCase("#foo", "/foo/document.html", true)]
            [TestCase("target#foo", "/foo/document.html", false)]
            [TestCase("target2#foo", "/foo/document.html", false)] // Sanity check
            public async Task DoesNotValidateHash(string link, string source, bool success)
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
                    new NormalizedPath(source),
                    new NormalizedPath(source.TrimStart('/')),
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("?", "/document.html", true)]
            [TestCase("?foo", "/document.html", true)]
            [TestCase("target?foo", "/document.html", true)]
            [TestCase("target2?foo", "/document.html", false)] // Sanity check
            [TestCase("?", "/foo/document.html", true)]
            [TestCase("?foo", "/foo/document.html", true)]
            [TestCase("target?foo", "/foo/document.html", false)]
            [TestCase("target2?foo", "/foo/document.html", false)] // Sanity check
            [TestCase("?#bar", "/document.html", true)]
            [TestCase("?foo#bar", "/document.html", true)]
            [TestCase("target?foo#bar", "/document.html", true)]
            [TestCase("target2?foo#bar", "/document.html", false)] // Sanity check
            [TestCase("?#bar", "/foo/document.html", true)]
            [TestCase("?foo#bar", "/foo/document.html", true)]
            [TestCase("target?foo#bar", "/foo/document.html", false)]
            [TestCase("target2?foo#bar", "/foo/document.html", false)] // Sanity check
            public async Task DoesNotValidateQueryString(string link, string source, bool success)
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
                    new NormalizedPath(source),
                    new NormalizedPath(source.TrimStart('/')),
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
                    context.AnalyzerResults.Count.ShouldBe(2); // +1 for the summary message
                }
            }

            [TestCase("mailto:foo@bar.com")] // Unescaped
            [TestCase("mailto:foo&#64;bar.com")] // Escaped
            public async Task DoesNotValidateMailToLinks(string mailto)
            {
                // Given
                TestDocument document = new TestDocument(
                    $"<html><head></head><body><a href=\"{mailto}\">foo</a></body></html>",
                    MediaTypes.Html);
                TestAnalyzerContext context = new TestAnalyzerContext(document);
                ValidateRelativeLinks validateRelativeLinks = new ValidateRelativeLinks();

                // When
                await validateRelativeLinks.AnalyzeAsync(context);

                // Then
                context.AnalyzerResults.ShouldBeEmpty();
            }
        }
    }
}