using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Shortcodes;

namespace Statiq.Web.Tests.Shortcodes
{
    [TestFixture]
    public class LinkShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : LinkShortcodeFixture
        {
            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "/foo/bar")]
            [TestCase("/foo/bar", "/foo/bar")]
            [TestCase("//foo/bar", "//foo/bar")]
            public void RendersLink(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path)
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }

            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "http://domain.com/foo/bar")]
            [TestCase("/foo/bar", "http://domain.com/foo/bar")]
            [TestCase("//foo/bar", "http://domain.com//foo/bar")]
            public void RendersLinkWithHost(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("IncludeHost", "true")
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }

            [TestCase("http://foo.com/bar", "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", "https://foo.com/bar")]
            [TestCase("foo/bar", "http://google.com/foo/bar")]
            [TestCase("/foo/bar", "http://google.com/foo/bar")]
            [TestCase("//foo/bar", "http://google.com//foo/bar")]
            public void RendersLinkWithAlternateHost(string path, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("Host", "google.com")
                };
                LinkShortcode shortcode = new LinkShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }

            [TestCase("index.html", "/index.html", null)]
            [TestCase("index.html", "/index.html", "true")]
            [TestCase("index.html", "index.html", "false")]
            public void ShouldRenderAbsoluteLink(string path, string expected, string makeAbsolute)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "Path", path }
                };
                if (makeAbsolute is object)
                {
                    args.Add("MakeAbsolute", makeAbsolute);
                }
                LinkShortcode shortcode = new LinkShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }

            [TestCase("/a/b/index.html", "/a/b", null)]
            [TestCase("/a/b/index.html", "/a/b", "false")]
            [TestCase("/a/b/index.html", "/a/b/", "true")]
            public void ShouldPreserveTrailingSlashForHiddenPages(string path, string expected, string hiddenPageTrailingSlash)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "Path", path },
                    { "HideIndexPages", "true" }
                };
                if (hiddenPageTrailingSlash is object)
                {
                    args.Add("HiddenPageTrailingSlash", hiddenPageTrailingSlash);
                }
                LinkShortcode shortcode = new LinkShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }
        }
    }
}