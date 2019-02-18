using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Wyam.Common.Execution;
using Wyam.Common.Meta;

namespace Wyam.Common.Shortcodes
{
    public static class ShortcodeExtensions
    {
        /// <summary>
        /// Validates that the arguments contain a single value and returns it.
        /// This will collapse keys and values into a single argument value so that "=" doesn't have to be
        /// escaped with quotes.
        /// </summary>
        /// <param name="args">The original shortcode arguments.</param>
        /// <returns>The single argument value.</returns>
        public static string SingleValue(this KeyValuePair<string, string>[] args) =>
            ToValueArray(args, 1)[0];

        /// <summary>
        /// Validates that the correct number of unnamed arguments have been used and returns them as an array.
        /// This will collapse keys and values into a single argument value so that "=" doesn't have to be
        /// escaped with quotes.
        /// </summary>
        /// <param name="args">The original shortcode arguments.</param>
        /// <param name="count">The count of expected arguments.</param>
        /// <returns>The argument values.</returns>
        public static string[] ToValueArray(this KeyValuePair<string, string>[] args, int count) =>
            ToValueArray(args, count, count);

        /// <summary>
        /// Validates that the correct number of arguments have been used and returns them as an array.
        /// This will collapse keys and values into a single argument value so that "=" doesn't have to be
        /// escaped with quotes.
        /// </summary>
        /// <param name="args">The original shortcode arguments.</param>
        /// <param name="minimumCount">The minimum count of expected arguments.</param>
        /// <param name="maximumCount">The maximum count of expected arguments.</param>
        /// <returns>The argument values.</returns>
        public static string[] ToValueArray(this KeyValuePair<string, string>[] args, int minimumCount, int maximumCount)
        {
            if (args.Length < minimumCount || args.Length > maximumCount)
            {
                throw new ArgumentException("Incorrect number of shortcode arguments");
            }
            return args.Select(x => x.Key != null ? $"{x.Key}={x.Value}" : x.Value).ToArray();
        }

        /// <summary>
        /// Converts the shortcode arguments into a dictionary of named parameters.
        /// This will match un-named positional parameters with their expected position
        /// after which named parameters will be included. If an un-named positional
        /// parameter follows named parameters and exception will be thrown.
        /// </summary>
        /// <param name="args">The original shortcode arguments.</param>
        /// <param name="context">The current execution context.</param>
        /// <param name="keys">The parameter names in expected order.</param>
        /// <returns>A dictionary containing the parameters and their values.</returns>
        public static ConvertingDictionary ToDictionary(this KeyValuePair<string, string>[] args, IExecutionContext context, params string[] keys)
        {
            ConvertingDictionary dictionary = new ConvertingDictionary(context);

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
