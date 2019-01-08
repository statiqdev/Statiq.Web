using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Wyam.Markdown.Tests
{
    public class AlternateTestMarkdownExtension : TestMarkdownExtension
    {
        public AlternateTestMarkdownExtension()
            : base("second")
        {
        }
    }
}