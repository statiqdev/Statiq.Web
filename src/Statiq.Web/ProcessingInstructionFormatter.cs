using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;

namespace Statiq.Web
{
    /// <summary>
    /// This uncomments shortcode processing instructions which currently get parsed as comments
    /// in AngleSharp. See https://github.com/Wyamio/Statiq/issues/784.
    /// This can be removed once https://github.com/AngleSharp/AngleSharp/pull/762 is merged.
    /// </summary>
    internal class ProcessingInstructionFormatter : IMarkupFormatter
    {
        private static readonly IMarkupFormatter Formatter = HtmlMarkupFormatter.Instance;

        public static readonly IMarkupFormatter Instance = new ProcessingInstructionFormatter();

        public string Attribute(IAttr attribute) => Formatter.Attribute(attribute);

        public string CloseTag(IElement element, bool selfClosing) => Formatter.CloseTag(element, selfClosing);

        public string Doctype(IDocumentType doctype) => Formatter.Doctype(doctype);

        public string OpenTag(IElement element, bool selfClosing) => Formatter.OpenTag(element, selfClosing);

        public string Text(string text) => Formatter.Text(text);

        public string Processing(IProcessingInstruction processing) => Formatter.Processing(processing);

        public string Comment(IComment comment)
        {
            if (comment.Data.StartsWith("?") && comment.Data.EndsWith("?"))
            {
                // This was probably a shortcode, so uncomment it
                return $"<{comment.Data}>";
            }
            return Formatter.Comment(comment);
        }
    }
}
