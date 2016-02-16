using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Tracing;

namespace Wyam
{
    /// <summary>
    /// Parses INI-like args.
    /// Key=Value
    /// </summary>
    class GlobalMetadataParser
    {
        /// <summary>
        /// Parses command line args and outputs dictionary.
        /// </summary>
        /// <remarks>
        /// TODO: decouple tracing from parsing.
        /// </remarks>
        /// <param name="args"></param>
        /// <returns></returns>
        public Dictionary<string, string> Parse(IEnumerable<string> args)
        {
            var metadata = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var pair = ParseSingle(arg);
                try {
                    metadata.Add(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    // catching Dictionary key insertion exceptions:
                    if (ex is ArgumentException || ex is ArgumentNullException)
                    {
                        Trace.Warning("Metadata arg \"{0}\" is dropped because of exception: {1}", arg, ex.Message);
                        if (Trace.Level == System.Diagnostics.SourceLevels.Verbose)
                            Trace.Warning("Stack trace:{0} {1}", Environment.NewLine, ex.StackTrace);
                    }
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
                    var pair = new KeyValuePair<string, string>
                        (arg.Substring(0, i).Trim(), arg.Substring(i + 1).Trim());
                    return pair;
                }
            }

            return new KeyValuePair<string, string>(arg.Trim().Replace("\\=", "="), null);
        }
    }
}
