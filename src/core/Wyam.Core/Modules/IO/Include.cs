using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Processes include statements to include files from the file system.
    /// </summary>
    /// <remarks>
    /// This module will look for include statements in the content of each document and
    /// will replace them with the content of the requested file from the file system.
    /// Include statements take the form <c>^"folder/file.ext"</c>. The given path will be
    /// converted to a <see cref="FilePath"/> and can be absolute or relative. If relative,
    /// it should be relative to the document source. You can escape the include syntax by
    /// prefixing the <c>^</c> with a forward slash <c>\</c>.
    /// </remarks>
    /// <category>Input/Output</category>
    public class Include : IModule
    {
        private bool _recursion = true;

        /// <summary>
        /// Specifies whether the include processing should be recursive. If <c>true</c> (which
        /// is the default), then include statements will also be processed in the content of
        /// included files recursively.
        /// </summary>
        /// <param name="recursion"><c>true</c> if included content should be recursively processed.</param>
        public Include WithRecursion(bool recursion = true)
        {
            _recursion = recursion;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                string content = input.Content;
                return ProcessIncludes(ref content, input.Source, context) ? context.GetDocument(input, content) : input;
            });
        }

        private bool ProcessIncludes(ref string content, FilePath source, IExecutionContext context)
        {
            bool modified = false;
            
            int start = 0;
            while (start >= 0)
            {
                start = content.IndexOf("^\"", start, StringComparison.Ordinal);
                if (start >= 0)
                {
                    // Check if the include is escaped
                    if (start > 0 && content[start - 1] == '\\')
                    {
                        modified = true;
                        content = content.Remove(start - 1, 1);
                        start++;
                    }
                    else
                    {
                        // This is a valid include
                        int end = content.IndexOf('\"', start + 2);
                        if (end > 0)
                        {
                            modified = true;

                            // Get the correct included path
                            FilePath includedPath = new FilePath(content.Substring(start + 2, end - (start + 2)));
                            if (includedPath.IsRelative)
                            {
                                if (source == null)
                                {
                                    throw new Exception($"Cannot include file at relative path {includedPath.FullPath} because document source is null");
                                }
                                includedPath = source.Directory.CombineFile(includedPath);
                            }

                            // Get and read the file content
                            IFile includedFile = context.FileSystem.GetFile(includedPath);
                            string includedContent = string.Empty;
                            if (!includedFile.Exists)
                            {
                                Trace.Warning($"Included file {includedFile.Path.FullPath} does not exist");
                            }
                            else
                            {
                                includedContent = includedFile.ReadAllText();
                            }

                            // Recursively process include statements
                            if (_recursion)
                            {
                                ProcessIncludes(ref includedContent, includedPath, context);
                            }

                            // Do the replacement
                            content = content.Remove(start, end - start + 1).Insert(start, includedContent);
                            start += includedContent.Length;
                        }
                    }
                }
            }

            return modified;
        }
    }
}