using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    public class ChildPages : Shortcode
    {
        public override async Task<IEnumerable<IDocument>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            foreach (IDocument child in document.GetChildren())
            {
                string description = child.GetString("Description");
                if (!string.IsNullOrEmpty(description))
                {
                    description = $"<p>{description}</p>";
                }
                IDocument result = context.CreateDocument(
                    await context.GetContentProviderAsync($"<h4><a href=\"{child.GetLink(context)}\">{child.GetTitle()}</a></h4>{description}"));
                results.Add(result);
            }
            return results;
        }
    }
}
