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
    public class FigureShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : FigureShortcodeFixture
        {
            [Test]
            public void RendersFigure()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "/a/b"),
                    new KeyValuePair<string, string>(null, "/c/d"),
                    new KeyValuePair<string, string>(null, "abc"),
                    new KeyValuePair<string, string>(null, "def"),
                    new KeyValuePair<string, string>(null, "ghi"),
                    new KeyValuePair<string, string>(null, "jkl"),
                    new KeyValuePair<string, string>(null, "100px"),
                    new KeyValuePair<string, string>(null, "200px")
                };
                FigureShortcode shortcode = new FigureShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, "foo bar", document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    @"<figure class=""jkl"">
  <a href=""/c/d"" target=""abc"" rel=""def"">
    <img src=""/a/b"" alt=""ghi"" height=""100px"" width=""200px"" />
  </a>
  <figcaption>foo bar</figcaption>
</figure>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void RendersFigureWithoutLink()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Src", "/a/b"),
                    new KeyValuePair<string, string>("Alt", "ghi"),
                    new KeyValuePair<string, string>("Class", "jkl"),
                    new KeyValuePair<string, string>("Height", "100px"),
                    new KeyValuePair<string, string>("Width", "200px")
                };
                FigureShortcode shortcode = new FigureShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, "foo bar", document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    @"<figure class=""jkl"">
  <img src=""/a/b"" alt=""ghi"" height=""100px"" width=""200px"" />
  <figcaption>foo bar</figcaption>
</figure>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotRenderLinkIfNoImage()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Link", "/c/d"),
                    new KeyValuePair<string, string>("Target", "abc"),
                    new KeyValuePair<string, string>("Rel", "def"),
                    new KeyValuePair<string, string>("Alt", "ghi"),
                    new KeyValuePair<string, string>("Class", "jkl"),
                    new KeyValuePair<string, string>("Height", "100px"),
                    new KeyValuePair<string, string>("Width", "200px")
                };
                FigureShortcode shortcode = new FigureShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, "foo bar", document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    @"<figure class=""jkl"">
  <figcaption>foo bar</figcaption>
</figure>",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
