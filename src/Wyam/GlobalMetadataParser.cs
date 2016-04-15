using System;
using System.Collections.Generic;

namespace Wyam
{
    [Serializable]
    public class MetadataParseException : Exception
    {
        public MetadataParseException(string message) : base(message) { }
        public MetadataParseException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Parses INI-like args.
    /// Key=Value
    /// </summary>
    public static class GlobalMetadataParser
    {
        /// <summary>
        /// Parses command line args and outputs dictionary.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<string, object> Parse(IEnumerable<string> args)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();
            foreach (var arg in args)
            {
                KeyValuePair<string, string> pair = ParsePair(arg);
                try
                {
                    metadata.Add(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException || ex is ArgumentNullException)
                    {
                        throw new MetadataParseException($"Error while adding metadata \"{arg}\" to dictionary: {ex.Message}", ex);
                    }
                }
            }

            return metadata;
        }

        private static KeyValuePair<string, string> ParsePair(string arg)
        {
            for (int i = 0; i < arg.Length; i++)
            {
                char c = arg[i];

                // capture pair when found delimiter '=' char
                // but don't capture if before it there is '\' char,
                // so "key\=value" is not a pair while
                // "key=value" is a pair.
                if (c == '=' && (i > 0 && arg[i - 1] != '\\'))
                {
                    string key = arg.Substring(0, i).Trim();
                    string value = arg.Substring(i + 1).Trim();

                    return new KeyValuePair<string, string>(key, value);
                }
            }

            return new KeyValuePair<string, string>(arg.Trim().Replace("\\=", "="), null);
        }
    }
}
