using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Xslt
{
    public class Xslt : IModule
    {
        private DocumentConfig _xsltPath;

        public Xslt(string xsltPath)
        {
            _xsltPath = (a, b) => xsltPath;

        }

        public Xslt(DocumentConfig xsltPath)
        {
            _xsltPath = xsltPath;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.Select(X =>
            {
                var xslt = new System.Xml.Xsl.XslCompiledTransform();
                string path = _xsltPath.Invoke<string>(X, context);
                path = Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));

                xslt.Load(path);
                using (var stream = X.GetStream())
                {
                    var str = new System.IO.StringWriter();
                    using (var writer = new System.Xml.XmlTextWriter(str))
                    {
                        xslt.Transform(System.Xml.XmlReader.Create(stream), writer);
                    }
                    return X.Clone(str.ToString());
                }
            });
        }
    }
}
