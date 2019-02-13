using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;

namespace Wyam.Common.Shortcodes
{
    /// <summary>
    /// Useful utility methods for shortcode implementations.
    /// </summary>
    public static class ShortcodeHelper
    {
        /// <summary>
        /// Converts the shortcode arguments into a dictionary of named parameters.
        /// This will match un-named positional parameters with their expected position
        /// after which named parameters will be included. If an un-named positional
        /// parameter follows named parameters and exception will be thrown.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="args">The original shortcode arguments.</param>
        /// <param name="keys">The parameter names in expected order.</param>
        /// <returns>A dictionary containing the parameters and their values.</returns>
        public static IMetadataDictionary GetArgsDictionary(IExecutionContext context, KeyValuePair<string, string>[] args, params string[] keys)
        {
            SimpleMetadataDictionary dictionary = new SimpleMetadataDictionary(context);
            bool nullKeyAllowed = true;
            for (int c = 0; c < args.Length; c++)
            {
                if (string.IsNullOrWhiteSpace(args[c].Key))
                {
                    if (!nullKeyAllowed)
                    {
                        throw new ShortcodeArgumentException("Unexpected positional shortcode argument");
                    }
                    dictionary.Add(keys[c], args[c].Value);
                }
                else
                {
                    if (dictionary.ContainsKey(args[c].Key))
                    {
                        throw new ShortcodeArgumentException("Duplicate shortcode parameter name", args[c].Key);
                    }
                    dictionary.Add(args[c].Key, args[c].Value);
                    nullKeyAllowed = false;
                }
            }
            return dictionary;
        }
    }
}
