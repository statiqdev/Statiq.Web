using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Templates
{
    /// <summary>
    /// Transforms input documents using a supplied XSLT template.
    /// </summary>
    /// <remarks>
    /// This module uses <see cref="System.Xml.Xsl.XslCompiledTransform"/> with default settings. This means that the
    /// XSLT <c>document()</c> function and embedded scripts are disabled. For more information
    /// see the <a href="https://msdn.microsoft.com/en-us/library/system.xml.xsl.xslcompiledtransform.aspx">MSDN documentation</a>.
    /// </remarks>
    /// <category>Templates</category>
    public class Xslt : IModule
    {
        private readonly DocumentConfig _xsltPath;
        private readonly IModule[] _xsltGeneration;

        /// <summary>
        /// Transforms input documents using a specified XSLT file from the file system.
        /// </summary>
        /// <param name="xsltPath">The path of the XSLT file to use.</param>
        public Xslt(FilePath xsltPath)
        {
            _xsltPath = (a, b) => xsltPath;
        }

        /// <summary>
        /// Transforms input documents using a specified XSLT file from the file system
        /// as provided by a delegate. This allows you to use different XSLT files depending
        /// on the input document.
        /// </summary>
        /// <param name="xsltPath">A delegate that should return a <see cref="FilePath"/> with the XSLT file to use.</param>
        public Xslt(DocumentConfig xsltPath)
        {
            _xsltPath = xsltPath;
        }

        /// <summary>
        /// Transforms input documents using the output content from the specified modules. The modules are executed for each input
        /// document with the current document as the input to the specified modules.
        /// </summary>
        /// <param name="modules">Modules that should output a single document containing the XSLT template in it's content.</param>
        public Xslt(params IModule[] modules)
        {
            _xsltGeneration = modules;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                try
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();

                    if (_xsltPath != null)
                    {
                        FilePath path = _xsltPath.Invoke<FilePath>(input, context);
                        if (path != null)
                        {
                            IFile file = context.FileSystem.GetInputFile(path);
                            if (file.Exists)
                            {
                                using (Stream fileStream = file.OpenRead())
                                {
                                    xslt.Load(XmlReader.Create(fileStream));
                                }
                            }
                        }
                    }
                    else if (_xsltGeneration != null)
                    {
                        IDocument xsltDocument = context.Execute(_xsltGeneration, new[] { input }).Single();
                        using (Stream stream = xsltDocument.GetStream())
                        {
                            xslt.Load(XmlReader.Create(stream));
                        }
                    }
                    using (Stream stream = input.GetStream())
                    {
                        StringWriter str = new StringWriter();
                        using (XmlTextWriter writer = new XmlTextWriter(str))
                        {
                            xslt.Transform(XmlReader.Create(stream), writer);
                        }
                        return context.GetDocument(input, context.GetContentStream(str.ToString()));
                    }
                }
                catch (Exception e)
                {
                    Trace.Error($"An {e.GetType().Name} occurred: {e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
