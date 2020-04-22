using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Shortcodes;

namespace Statiq.Web.Tests.Shortcodes
{
    [TestFixture]
    public class GistShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : GistShortcodeFixture
        {
            [Test]
            public void RendersGist()
            {
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "abc"),
                    new KeyValuePair<string, string>(null, "def"),
                    new KeyValuePair<string, string>(null, "ghi"),
                };
                GistShortcode shortcode = new GistShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, null, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("<script src=\"//gist.github.com/def/abc.js?file=ghi\" type=\"text/javascript\"></script>");
            }

            [Test]
            public void RendersGistWithoutUsername()
            {
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "abc"),
                    new KeyValuePair<string, string>("File", "ghi"),
                };
                GistShortcode shortcode = new GistShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, null, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("<script src=\"//gist.github.com/abc.js?file=ghi\" type=\"text/javascript\"></script>");
            }

            [Test]
            public void RendersGistWithoutFile()
            {
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "abc"),
                    new KeyValuePair<string, string>(null, "def")
                };
                GistShortcode shortcode = new GistShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, null, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("<script src=\"//gist.github.com/def/abc.js\" type=\"text/javascript\"></script>");
            }
        }
    }
}
