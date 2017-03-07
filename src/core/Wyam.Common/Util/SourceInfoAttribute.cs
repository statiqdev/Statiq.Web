using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    /// <summary>
    /// A clever trick to get source info for declarations during reflection.
    /// From http://stackoverflow.com/a/17998371/807064.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public sealed class SourceInfoAttribute : Attribute
    {
        /// <summary>
        /// Creates an instance of the attribute.
        /// </summary>
        /// <param name="filePath">The automatically populated file path (don't supply this manually)</param>
        /// <param name="lineNumber">The automatically populated source file line number (don't supply this manually)</param>
        public SourceInfoAttribute([CallerFilePath]string filePath = null, [CallerLineNumber]int lineNumber = 0)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// The line number of the attribute in the source file.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The file path of the source file.
        /// </summary>
        public string FilePath { get; }

    }
}
