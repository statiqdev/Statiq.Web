using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Extracts the content of a Sidecar file for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML in a Sidecar file.
    /// First, an attempt is made to find the specified sidecar file for each input document. Once found, the
    /// content in this file is passed to the specified child module(s). Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child module(s) result
    /// in more than one output document, multiple clones of the input document will be made for each one.
    /// The output document content is set to the original input document content.
    /// </remarks>
    /// <metadata cref="Keys.SourceFilePath" usage="Input">
    /// Used as the default location at which to search for sidecar files for a given document.
    /// </metadata>
    /// <category>Control</category>
    public class Sidecar : ContainerModule
    {
        private readonly DocumentConfig _sidecarPath;

        /// <summary>
        /// Searches for sidecar files at the same path as the input document SourceFilePath with the additional extension .meta.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(params IModule[] modules)
            : this(".meta", (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Searches for sidecar files at the same path as the input document SourceFilePath with the additional extension .meta.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(IEnumerable<IModule> modules)
            : this(".meta", modules)
        {
        }

        /// <summary>
        /// Searches for sidecar files at the same path as the input document SourceFilePath with the specified additional extension.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="extension">The extension to search.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(string extension, params IModule[] modules)
            : this(extension, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Searches for sidecar files at the same path as the input document SourceFilePath with the specified additional extension.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="extension">The extension to search.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(string extension, IEnumerable<IModule> modules)
            : base(modules)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(extension));
            }

            _sidecarPath = (d, c) => d.FilePath(Common.Meta.Keys.SourceFilePath)?.AppendExtension(extension);
        }

        /// <summary>
        /// Uses a delegate to describe where to find the sidecar file for each input document.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="sidecarPath">A delegate that returns a <see cref="FilePath"/> with the desired sidecar path.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(DocumentConfig sidecarPath, params IModule[] modules)
            : this(sidecarPath, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Uses a delegate to describe where to find the sidecar file for each input document.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="sidecarPath">A delegate that returns a <see cref="FilePath"/> with the desired sidecar path.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public Sidecar(DocumentConfig sidecarPath, IEnumerable<IModule> modules)
            : base(modules)
        {
            if (sidecarPath == null)
            {
                throw new ArgumentNullException(nameof(sidecarPath));
            }

            _sidecarPath = sidecarPath;
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            context.ForEach(inputs, input =>
            {
                FilePath sidecarPath = _sidecarPath.Invoke<FilePath>(input, context);
                if (sidecarPath != null)
                {
                    IFile sidecarFile = context.FileSystem.GetInputFile(sidecarPath.FullPath);
                    if (sidecarFile.Exists)
                    {
                        string sidecarContent = sidecarFile.ReadAllText();
                        foreach (IDocument result in context.Execute(this, new[] { context.GetDocument(input, context.GetContentStream(sidecarContent)) }))
                        {
                            results.Add(context.GetDocument(input, result));
                        }
                    }
                    else
                    {
                        results.Add(input);
                    }
                }
                else
                {
                    results.Add(input);
                }
            });
            return results;
        }
    }
}
