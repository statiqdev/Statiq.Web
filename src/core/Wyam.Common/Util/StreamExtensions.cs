using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Extension methods for use with <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Creates a <see cref="StreamWriter"/> for the specified stream. The
        /// biggest difference between this and creating a <see cref="StreamWriter"/>
        /// directly is that the new <see cref="StreamWriter"/> will default to
        /// leaving the underlying stream open on disposal. Remember to flush the
        /// returned writer after all data have been written.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the underlying stream open on disposal.</param>
        /// <returns>A new <see cref="StreamWriter"/> for the specified stream.</returns>
        public static StreamWriter GetWriter(this Stream stream, bool leaveOpen = true) =>
            new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen);
    }
}
