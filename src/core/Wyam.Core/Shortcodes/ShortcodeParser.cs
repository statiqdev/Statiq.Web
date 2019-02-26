using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Shortcodes;
using Wyam.Core.Util;

namespace Wyam.Core.Shortcodes
{
    /// <summary>
    /// Parses a stream looking for shortcodes. This class is not thread-safe and maintains
    /// state. A new instance should be created for each stream.
    /// </summary>
    internal class ShortcodeParser
    {
        public const string DefaultPreRenderStartDelimiter = "<?!";
        public const string DefaultPreRenderEndDelimiter = "?>";

        public const string DefaultPostRenderStartDelimiter = "<?#";
        public const string DefaultPostRenderEndDelimiter = "?>";

        private readonly Delimiter _startDelimiter;
        private readonly Delimiter _endDelimiter;
        private readonly IReadOnlyShortcodeCollection _shortcodes;

        public ShortcodeParser(string startDelimiter, string endDelimiter, IReadOnlyShortcodeCollection shortcodes)
        {
            _startDelimiter = new Delimiter(startDelimiter, true);
            _endDelimiter = new Delimiter(endDelimiter, false);
            _shortcodes = shortcodes;
        }

        /// <summary>
        /// Identifies shortcode locations in a stream.
        /// </summary>
        /// <param name="stream">The stream to parse. This method will not dispose the passed-in stream.</param>
        /// <returns>All of the shortcode locations in the stream.</returns>
        public List<ShortcodeLocation> Parse(Stream stream)
        {
            List<ShortcodeLocation> locations = new List<ShortcodeLocation>();

            CurrentTag currentTag = null;
            ShortcodeLocation shortcode = null;
            StringBuilder content = null;

            using (TextReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                int r;
                int i = 0;
                while ((r = reader.Read()) != -1)
                {
                    char c = (char)r;

                    // Look for delimiters and tags
                    if (currentTag == null && shortcode == null)
                    {
                        // Searching for open tag start delimiter
                        if (_startDelimiter.Locate(c, false))
                        {
                            currentTag = new CurrentTag(i - (_startDelimiter.Text.Length - 1));
                        }
                    }
                    else if (currentTag != null && shortcode == null)
                    {
                        // Searching for open tag end delimiter
                        currentTag.Content.Append(c);
                        if (_endDelimiter.Locate(c, false))
                        {
                            // Is this self-closing?
                            if (currentTag.Content[currentTag.Content.Length - _endDelimiter.Text.Length - 1] == '/')
                            {
                                // Self-closing
                                shortcode = GetShortcodeLocation(
                                    currentTag.FirstIndex,
                                    currentTag.Content.ToString(0, currentTag.Content.Length - _endDelimiter.Text.Length - 1));
                                shortcode.Finish(i);
                                locations.Add(shortcode);
                                shortcode = null;
                            }
                            else
                            {
                                // Look for a closing tag
                                shortcode = GetShortcodeLocation(
                                    currentTag.FirstIndex,
                                    currentTag.Content.ToString(0, currentTag.Content.Length - _endDelimiter.Text.Length));
                                content = new StringBuilder();
                            }

                            currentTag = null;
                        }
                    }
                    else if (currentTag == null && shortcode != null)
                    {
                        content.Append(c);

                        // Searching for close tag start delimiter
                        if (_startDelimiter.Locate(c, true))
                        {
                            currentTag = new CurrentTag(i);
                        }
                    }
                    else
                    {
                        currentTag.Content.Append(c);

                        // Searching for close tag end delimiter
                        if (_endDelimiter.Locate(c, false))
                        {
                            // Get the name of this shortcode close tag
                            string name = currentTag.Content.ToString(
                                0,
                                currentTag.Content.Length - _endDelimiter.Text.Length)
                                .Trim();
                            if (name.Any(x => char.IsWhiteSpace(x)))
                            {
                                throw new ShortcodeParserException("Closing shortcode tags should only consist of the shortcode name");
                            }

                            // Make sure it's the same name
                            if (name.Equals(shortcode.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                // If the content is contained within a processing instruction, trim that
                                string shortcodeContent = content.ToString(0, content.Length - _startDelimiter.Text.Length - 1);
                                string trimmedContent = shortcodeContent.Trim();
                                if (trimmedContent.StartsWith("<?") && trimmedContent.EndsWith("?>"))
                                {
                                    shortcodeContent = trimmedContent.Substring(2, trimmedContent.Length - 4);
                                }

                                shortcode.Content = shortcodeContent;
                                shortcode.Finish(i);
                                locations.Add(shortcode);

                                shortcode = null;
                                content = null;
                            }
                            else
                            {
                                // It wasn't the same name, so add the tag content to the running content
                                content.Append(currentTag.Content.ToString());
                            }

                            currentTag = null;
                        }
                    }

                    i++;
                }

                if (shortcode != null)
                {
                    throw new ShortcodeParserException($"The shortcode {shortcode.Name} was not terminated");
                }
            }

            return locations;
        }

        private ShortcodeLocation GetShortcodeLocation(int firstIndex, string tagContent)
        {
            // Trim whitespace
            tagContent = tagContent.Trim();
            if (tagContent.Length < 1)
            {
                throw new ShortcodeParserException("Shortcode must have a name");
            }

            // Get the name and arguments
            string name;
            KeyValuePair<string, string>[] arguments;
            int nameLength = tagContent.IndexOf(' ');
            if (nameLength < 0)
            {
                name = tagContent;
                arguments = Array.Empty<KeyValuePair<string, string>>();
            }
            else
            {
                name = tagContent.Substring(0, nameLength);
                arguments = SplitArguments(tagContent, nameLength + 1).ToArray();
            }

            // Try to get the shortcode
            if (!_shortcodes.Contains(name))
            {
                throw new ShortcodeParserException($"A shortcode with the name {name} was not found");
            }

            return new ShortcodeLocation(firstIndex, name, arguments);
        }

        public static IEnumerable<KeyValuePair<string, string>> SplitArguments(string arguments, int start)
        {
            int valueStart = -1;
            int valueLength = 0;
            int keyStart = -1;
            int keyLength = 0;
            bool inQuotes = false;
            for (int i = start; i < arguments.Length; i++)
            {
                char c = arguments[i];

                if (c == '\"' && (i == 0 || arguments[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;

                    if (!inQuotes && valueLength > 0 && (i == arguments.Length - 1 || arguments[i + 1] != '='))
                    {
                        yield return new KeyValuePair<string, string>(
                            keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                            valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
                        valueStart = -1;
                        valueLength = 0;
                        keyStart = -1;
                        keyLength = 0;
                        continue;
                    }

                    if (inQuotes && valueStart == -1 && i < arguments.Length - 1)
                    {
                        valueStart = i + 1;
                        continue;
                    }

                    // If it's a quote at the end of the string, treat as a normal char
                }

                if (inQuotes)
                {
                    valueLength++;
                }
                else if (c == '=')
                {
                    keyStart = valueStart;
                    keyLength = i > 0 && arguments[i - 1] == '\"' ? valueLength - 1 : valueLength;
                    valueStart = -1;
                    valueLength = 0;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (valueLength > 0)
                    {
                        yield return new KeyValuePair<string, string>(
                            keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                            valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
                    }
                    valueLength = 0;
                    valueStart = -1;
                    keyLength = 0;
                    keyStart = -1;
                }
                else
                {
                    if (valueStart == -1)
                    {
                        valueStart = i;
                        valueLength = 1;
                    }
                    else
                    {
                        valueLength++;
                    }
                }
            }

            if (valueLength > 0)
            {
                yield return new KeyValuePair<string, string>(
                    keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                    valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
            }
        }
    }
}
