using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.IO
{
    /// <summary>
    /// Includes a file from the virtual file system.
    /// </summary>
    /// <remarks>
    /// This shortcode accepts a single argument value with the path to the file to include.
    /// </remarks>
    public class Include : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Incorrect number of arguments");
            }
            if (args[0].Key != null || args[0].Value == null)
            {
                throw new ArgumentException("Incorrect arguments");
            }

            IFile file = context.FileSystem.GetInputFile(new FilePath(args[0].Value));
            return context.GetShortcodeResult(file.Exists ? file.OpenRead() : null);
        }
    }
}
