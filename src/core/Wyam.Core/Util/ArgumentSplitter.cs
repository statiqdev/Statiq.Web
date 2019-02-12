using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Util
{
    /// <summary>
    /// Splits a string into quoted arguments delimited by spaces.
    /// </summary>
    /// <remarks>
    /// From http://stackoverflow.com/a/298990/807064
    /// </remarks>
    public static class ArgumentSplitter
    {
        /// <summary>
        /// Splits a string into quoted arguments delimited by spaces.
        /// </summary>
        /// <param name="arguments">The full string to split into arguments.</param>
        /// <returns>Each quoted argument as delimited in the original string by spaces.</returns>
        public static IEnumerable<string> Split(string arguments)
        {
            bool inQuotes = false;

            return arguments
                .Split(c =>
                {
                    if (c == '\"')
                    {
                        inQuotes = !inQuotes;
                    }

                    return !inQuotes && c == ' ';
                })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }

        private static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2)
                && (input[0] == quote) && (input[input.Length - 1] == quote))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }
    }
}
