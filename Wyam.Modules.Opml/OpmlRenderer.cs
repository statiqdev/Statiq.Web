using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.Opml
{
    public class OpmlRenderer : IModule
    {
        OpmlDoc _doc = new OpmlDoc();

        public int _levelFilter { get; set; } = 0;

        public OpmlRenderer()
        {

        }

        public OpmlRenderer(int level)
        {
            _levelFilter = level;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.SelectMany((IDocument input) =>
            {
                var opml = new OpmlDoc();
                opml.LoadFromXML(input.Content);

                var docs = new List<IDocument>();

                return opml.Where(x => x.Level >= _levelFilter).Select(o =>
                {
                    var metadata = o.Attributes.Select(x => new KeyValuePair<string, object>(x.Key, x.Value));
                    return input.Clone(o.Text, metadata);
                });
            });
        }
    }
}
