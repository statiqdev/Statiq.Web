using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Core.Shortcodes.Html;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Shortcodes.Html
{
    [TestFixture]
    public class LinkFixture : BaseFixture
    {
        public class ExecuteTests : LinkFixture
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
                context.AddTypeConversion<string, FilePath>(x => new FilePath(x));
                context.AddTypeConversion<string, bool>(x => bool.Parse(x));
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path)
                };
                Link shortcode = new Link();

                // When
                IShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                using (TextReader reader = new StreamReader(result.Stream))
                {
                    reader.ReadToEnd().ShouldBe(expected);
                }
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
                context.AddTypeConversion<string, FilePath>(x => new FilePath(x));
                context.AddTypeConversion<string, bool>(x => bool.Parse(x));
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("IncludeHost", "true")
                };
                Link shortcode = new Link();

                // When
                IShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                using (TextReader reader = new StreamReader(result.Stream))
                {
                    reader.ReadToEnd().ShouldBe(expected);
                }
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
                context.AddTypeConversion<string, FilePath>(x => new FilePath(x));
                context.AddTypeConversion<string, bool>(x => bool.Parse(x));
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Path", path),
                    new KeyValuePair<string, string>("Host", "google.com")
                };
                Link shortcode = new Link();

                // When
                IShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                using (TextReader reader = new StreamReader(result.Stream))
                {
                    reader.ReadToEnd().ShouldBe(expected);
                }
            }
        }
    }
}
