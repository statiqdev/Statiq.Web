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
            IFile file = context.FileSystem.GetInputFile(new FilePath(args.SingleValue()));
            return context.GetShortcodeResult(file.Exists ? file.OpenRead() : null);
        }
    }
}
