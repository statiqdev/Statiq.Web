using System;
using System.Collections.Generic;

namespace Wyam.Core.Meta
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
    public class GlobalMetadataParser
    {
        /// <summary>
        /// Parses command line args and outputs dictionary.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        /// <returns></returns>
        public SimpleMetadata Parse(IEnumerable<string> args)
        {
            var metadata = new SimpleMetadata();
            foreach (var arg in args)
            {
                var pair = ParseSingle(arg);
                try
                {
                    metadata.Add(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException || ex is ArgumentNullException)
                        throw new MetadataParseException(string.Format("Error while adding metadata \"{0}\" to dictionary: {1}", arg, ex.Message), ex);
                }
            }

            return metadata;
        }

        private KeyValuePair<string, string> ParseSingle(string arg)
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
                    var key = arg.Substring(0, i).Trim();
                    var value = arg.Substring(i + 1).Trim();

                    return new KeyValuePair<string, string>(key, value);
                }
            }

            return new KeyValuePair<string, string>(arg.Trim().Replace("\\=", "="), null);
        }
    }
}
