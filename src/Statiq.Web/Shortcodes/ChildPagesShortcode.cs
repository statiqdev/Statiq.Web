using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Outputs the child pages of the current document.
    /// </summary>
    public class ChildPagesShortcode : SyncContentShortcode
    {
        public override string Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<div>");
            builder.AppendLine(@"<h3>Child Pages</h3>");
            builder.AppendLine(@"<ul>");
            foreach (IDocument child in document.GetChildren())
            {
                builder.AppendLine(@"<li>");
                builder.AppendLine($@"<h4><a href=""{child.GetLink()}"">{child.GetTitle()}</a></h4>");
                string description = child.GetString("Description");
                if (!string.IsNullOrEmpty(description))
                {
                    builder.AppendLine($@"<p class=""card-text"">{description}</p>");
                }
                builder.AppendLine("</li>");
            }
            builder.AppendLine("</ul>");
            builder.AppendLine("</div>");
            return builder.ToString();
        }
    }
}
