using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Xslt2
{
    /// <summary>
    /// Transforms input documents using a supplied XSLT 2 template. Unlike the Xslt module,
    /// this one uses a library to support XSLT 2 and therefore needs to be included as a
    /// package.
    /// </summary>
    /// <remarks>
    /// This module uses Saxon with default settings. For more information 
    /// see the <a href="http://www.saxonica.com">Saxonica website</a>.
    /// </remarks>
    /// <category>Templates</category>
    public class Xslt2 : IModule
    {
        private readonly DocumentConfig _xsltPath;
        private readonly IModule[] _xsltGeneration;

        /// <summary>
        /// Transforms input documents using a specified XSLT file from the file system.
        /// </summary>
        /// <param name="xsltPath">The path of the XSLT file to use.</param>
        public Xslt2(FilePath xsltPath)
        {
            if (xsltPath == null)
                throw new ArgumentNullException(nameof(xsltPath));
            _xsltPath = (a, b) => xsltPath;
        }

        /// <summary>
        /// Transforms input documents using a specified XSLT file from the file system
        /// as provided by a delegate. This allows you to use different XSLT files depending
        /// on the input document.
        /// </summary>
        /// <param name="xsltPath">A delegate that should return a <see cref="FilePath"/> with the XSLT file to use.</param>
        public Xslt2(DocumentConfig xsltPath)
        {
            if (xsltPath == null)
                throw new ArgumentNullException(nameof(xsltPath));
            _xsltPath = xsltPath;
        }

        /// <summary>
        /// Transforms input documents using the output content from the specified modules. The modules are executed for each input
        /// document with the current document as the input to the specified modules.
        /// </summary>
        /// <param name="modules">Modules that should output a single document containing the XSLT template in it's content.</param>
        public Xslt2(params IModule[] modules)
        {
            if (modules == null)
                throw new ArgumentNullException(nameof(modules));
            _xsltGeneration = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                try
                {
                    Saxon.Api.Processor processor = new Saxon.Api.Processor();
                    Saxon.Api.XsltCompiler xslt = processor.NewXsltCompiler();
                    Saxon.Api.XsltTransformer transformer;
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
                                    Saxon.Api.XsltExecutable executable = xslt.Compile(fileStream);
                                    transformer = executable.Load();
                                }
                            }
                            else
                            {
                                throw new FileNotFoundException("Couldn't find XSLT file", file.ToString());
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Provided file path was not valid");
                        }
                    }
                    else if (_xsltGeneration != null)
                    {
                        IDocument xsltDocument = context.Execute(_xsltGeneration, new[] { input }).Single();
                        using (Stream stream = xsltDocument.GetStream())
                        {

                            var xml = XmlReader.Create(stream);
                            xslt.BaseUri  = new Uri(string.IsNullOrEmpty( xml.BaseURI)?"http://temp.org": xml.BaseURI);
                            Saxon.Api.XsltExecutable executable = xslt.Compile(xml);
                            transformer = executable.Load();
                        }
                    }
                    else
                    {
                        //Should never happen, because of null check in Constructor.
                        throw new InvalidOperationException();
                    }

                    using (Stream stream = input.GetStream())
                    {
                        var documentBuilder = processor.NewDocumentBuilder();
                        documentBuilder.BaseUri = xslt.BaseUri;
                        var xdmNode = documentBuilder.Build(stream);
                        transformer.InitialContextNode = xdmNode;
                        
                        using (System.IO.StringWriter writer = new StringWriter())
                        {
                            Saxon.Api.Serializer serializer = new Saxon.Api.Serializer();
                            serializer.SetOutputWriter(writer);
                            transformer.Run(serializer);
                            return context.GetDocument(input, writer.ToString());
                        }
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
