using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Wyam.Common;

namespace Wyam.Modules.Html
{
    public class AutoLink : IModule
    {
        // Key = text to replace, Value = url
        private readonly Func<IDocument, IDictionary<string, string>> _links;

        public AutoLink(IDictionary<string, string> links)
        {
            _links = x => links;
        }

        public AutoLink(Func<IDocument, IDictionary<string, string>> links)
        {
            _links = links;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();
            return inputs.Select(x =>
            {
                IDictionary<string, string> links = _links(x);
                try
                {
                    IHtmlDocument htmlDocument = parser.Parse(x.Content);


                    return x;
                }
                catch (Exception ex)
                {
                    context.Trace.Warning("Exception while parsing HTML for {0}: {1}", x.Source, ex.Message);
                    return x;
                }
            });
        }
    }
}
