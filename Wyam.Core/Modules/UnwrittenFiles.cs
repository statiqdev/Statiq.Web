using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    /// <summary>
    /// Outputs only those documents that have not yet been written to the file system.
    /// </summary>
    /// <remarks>
    /// The constructors and file resolution logic follows the same semantics as <see cref="WriteFiles"/>. 
    /// This module is useful for eliminating documents from the pipeline on subsequent runs depending 
    /// on if they've already been written to disk. For example, you might want to put this module 
    /// right after <see cref="ReadFiles"/> for a pipeline that does a lot of expensive image processing since 
    /// there's no use in processing images that have already been processed. Note that only the 
    /// file name is checked and that this module cannot determine if the content would have been 
    /// the same had a document not been removed from the pipeline. Also note that <strong>you should only
    /// use this module if you're sure that no other pipelines rely on the output documents</strong>. Because 
    /// this module removes documents from the pipeline, those documents will never reach the 
    /// end of the pipeline and any other modules or pages that rely on them (for example, an 
    /// image directory) will not be correct.
    /// </remarks>
    /// <metadata name="DestinationFilePath" type="string">The full absolute path (including file name) 
    /// of the destination file.</metadata>
    /// <metadata name="DestinationFilePathBase" type="string">The full absolute path (including file name) 
    /// of the destination file without the file extension.</metadata>
    /// <metadata name="DestinationFileBase" type="string">The file name without any extension. Equivalent 
    /// to <c>Path.GetFileNameWithoutExtension(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileExt" type="string">The extension of the file. Equivalent 
    /// to <c>Path.GetExtension(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileName" type="string">The full file name. Equivalent 
    /// to <c>Path.GetFileName(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileDir" type="string">The full absolute directory of the file. 
    /// Equivalent to <c>Path.GetDirectoryName(DestinationFilePath)</c>.</metadata>
    /// <metadata name="RelativeFilePath" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder.</metadata>
    /// <metadata name="RelativeFilePathBase" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir" type="string">The relative directory of the file 
    /// from the Wyam input folder.</metadata>
    /// <category>Input/Output</category>
    public class UnwrittenFiles : WriteFiles
    {
        /// <summary>
        /// Uses a delegate to describe where to write the content of each document. 
        /// The output of the function should be either a full path to the disk 
        /// location (including file name) or a path relative to the output folder.
        /// </summary>
        /// <param name="path">A delegate that returns a <c>string</c> with the desired path.</param>
        public UnwrittenFiles(DocumentConfig path) : base(path)
        {
        }

        /// <summary>
        /// Writes the document content to disk with the specified extension with the same 
        /// base file name and relative path as the input file. This requires metadata 
        /// for <c>RelativeFilePath</c> to be set (which is done by default by the <see cref="ReadFiles"/> module).
        /// </summary>
        /// <param name="extension">The extension to use for writing the file.</param>
        public UnwrittenFiles(string extension) : base(extension)
        {
        }

        /// <summary>
        /// Writes the document content to disk with the same file name and relative path 
        /// as the input file. This requires metadata for <c>RelativeFilePath</c> to be set 
        /// (which is done by default by the <see cref="ReadFiles"/> module).
        /// </summary>
        public UnwrittenFiles()
        {
        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                if (ShouldProcess(input, context))
                {
                    string path = GetPath(input, context);
                    if (path != null)
                    {
                        string pathDirectory = Path.GetDirectoryName(path);
                        if ((pathDirectory != null && Directory.Exists(pathDirectory) && File.Exists(path)) || (pathDirectory == null && File.Exists(path)))
                        {
                            return null;
                        }
                    }
                }
                return input;
            }).Where(x => x != null);
        }
    }
}
