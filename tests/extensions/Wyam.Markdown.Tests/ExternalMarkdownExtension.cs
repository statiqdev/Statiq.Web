using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Wyam.Markdown.Tests
{
    public class ExternalMarkdownExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // Make sure we don't have a delegate twice
            pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
            pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
        }

        private static void PipelineOnDocumentProcessed(MarkdownDocument document)
        {
            foreach (var node in document.Descendants())
            {
                if (node is Inline)
                {
                    var link = node as LinkInline;
                    if (link != null && link.IsImage)
                    {
                        link.GetAttributes().AddClass("ui spaced image");
                    }
                }
            }
        }
    }
}
