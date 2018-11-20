using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Wyam.Markdown.Tests
{
    public class TestMarkdownExtension : IMarkdownExtension
    {
        public bool ReceivedSetup { get; set; }

        public string LinkClassToAdd { get; set; }

        public TestMarkdownExtension()
        {
            LinkClassToAdd = "ui spaced image";
        }

        public TestMarkdownExtension(string linkClassToAdd)
        {
            LinkClassToAdd = linkClassToAdd;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            ReceivedSetup = true;

            // Make sure we don't have a delegate twice
            pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
            pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            ReceivedSetup = true;
        }

        private void PipelineOnDocumentProcessed(MarkdownDocument document)
        {
            foreach (MarkdownObject node in document.Descendants())
            {
                if (node is Inline)
                {
                    LinkInline link = node as LinkInline;
                    if (link != null && link.IsImage)
                    {
                        link.GetAttributes().AddClass(LinkClassToAdd);
                    }
                }
            }
        }
    }
}
