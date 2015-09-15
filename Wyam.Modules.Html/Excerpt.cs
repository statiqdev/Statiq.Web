using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Wyam.Common;
using IDocument = Wyam.Common.IDocument;

namespace Wyam.Modules.Html
{
    public class Excerpt : IModule
    {
        private string _querySelector = "p";
        private string _metadataKey = "Excerpt";
        private bool _outerHtml = true;

        public Excerpt()
        {
        }

        public Excerpt(string querySelector)
        {
            _querySelector = querySelector;
        }

        public Excerpt SetMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        public Excerpt WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        public Excerpt GetOuterHtml(bool outerHtml)
        {
            _outerHtml = outerHtml;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlQuery query = new HtmlQuery(_querySelector).First();
            if (_outerHtml)
            {
                query.GetOuterHtml(_metadataKey);
            }
            else
            {
                query.GetInnerHtml(_metadataKey);
            }
            return query.Execute(inputs, context);
        }
    }
}
