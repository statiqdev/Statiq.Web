using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Extracts the content of a Sidecar file for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML in a Sidecar file
    /// in a file. First, for each File it is searched for a Sidecar file. Once found, the 
    /// content in this file is passed to the specified child modules. Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child modules result 
    /// in more than one output document, multiple clones of the input document will be made for each one. 
    /// The output document content is set to the original content.
    /// </remarks>
    /// <category>Control</category>
    public class Sidecar : IModule
    {
        private readonly string _extension;
        private readonly IModule[] _modules;

        /// <summary>
        /// Uses the default delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public Sidecar(params IModule[] modules)
        {
            _extension = ".meta";
            _modules = modules;
        }

        /// <summary>
        /// Uses the specified delimiter string and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="extension">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public Sidecar(string extension, params IModule[] modules)
        {
            _extension = extension;
            _modules = modules;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {


                FilePath sourceFilePath = input.FilePath(Keys.SourceFilePath);
                if (sourceFilePath != null)
                {
                    IFile sidecarFile = context.FileSystem.GetInputFile(sourceFilePath.FullPath + _extension);
                    if (sidecarFile.Exists)
                    {
                        string frontMatter = sidecarFile.ReadAllText();
                        foreach (IDocument result in context.Execute(_modules, new[] { context.GetDocument(input, frontMatter) }))
                        {
                            yield return context.GetDocument(input, result);
                        }
                    }
                    else
                    {
                        yield return input;
                    }
                }
                else
                {
                    yield return input;
                }
            }
        }
    }
}
