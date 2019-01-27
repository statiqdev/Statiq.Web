using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Testing
{
    /// <summary>
    /// Extensions for testing strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts all <c>\r\n</c> pairs to <c>\n</c>.
        /// </summary>
        /// <param name="str">The string to normalize.</param>
        /// <returns>The normalized string.</returns>
        public static string NormalizeLineEndings(this string str) => str.Replace("\r\n", "\n");
    }
}
