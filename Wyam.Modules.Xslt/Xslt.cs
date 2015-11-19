using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Xslt
{
    /// <summary>
    /// Transforms the input documnats (which should be valid xml) using the provided xslt.
    /// </summary>
    /// <remarks>
    /// This Module uses the <see cref="System.Xml.Xsl.XslCompiledTransform"/> with default settings. So document() function and embedded scripts are disabled. For more information see MSDN Doku.
    /// </remarks>
    public class Xslt : IModule
    {
        private DocumentConfig _xsltPath;
        private IModule[] _xsltGeneration;

        /// <summary>
        /// Creates an instance of Xslt module.
        /// </summary>
        /// <param name="xsltPath">The Path where the xslt file is present.</param>
        public Xslt(string xsltPath)
        {
            _xsltPath = (a, b) => xsltPath;

        }

        /// <summary>
        /// Creates an instance of Xslt module.
        /// </summary>
        /// <param name="xsltPath">A delegate that should return a path to an xslt file for every document.</param>
        public Xslt(DocumentConfig xsltPath)
        {
            _xsltPath = xsltPath;
        }

        /// <summary>
        /// Creates an instance of Xslt module.
        /// </summary>
        /// <param name="moduls">A set of modules that should generate a singel document which will be used as an xslt file.</param>
        public Xslt(params IModule[] moduls)
        {
            _xsltGeneration = moduls;
        }



        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(X =>
            {
                try
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();

                    if (_xsltPath != null)
                    {
                        string path = _xsltPath.Invoke<string>(X, context);
                        path = Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                        xslt.Load(path);
                    }
                    else if (_xsltGeneration != null)
                    {
                        IDocument xsltDocument = context.Execute(_xsltGeneration).Single();
                        using (var stream = xsltDocument.GetStream())
                        {
                            xslt.Load(System.Xml.XmlReader.Create(stream));
                        }
                    }
                    using (var stream = X.GetStream())
                    {
                        StringWriter str = new System.IO.StringWriter();
                        using (var writer = new System.Xml.XmlTextWriter(str))
                        {
                            xslt.Transform(System.Xml.XmlReader.Create(stream), writer);
                        }
                        return X.Clone(str.ToString());
                    }

                }
                catch (Exception e)
                {
                    context.Trace.Error($"An {e.GetType().Name} occured.\n\t{e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
