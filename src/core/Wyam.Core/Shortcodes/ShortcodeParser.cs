using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Shortcodes;
using Wyam.Core.Util;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeParser
    {
        public const string DefaultStartDelimiter = "{{%";
        public const string DefaultEndDelimiter = "%}}";

        private readonly Delimiter _startDelimiter;
        private readonly Delimiter _endDelimiter;
        private readonly IReadOnlyShortcodeCollection _shortcodes;

        public ShortcodeParser(IReadOnlyShortcodeCollection shortcodes)
            : this(DefaultStartDelimiter, DefaultEndDelimiter, shortcodes)
        {
        }

        public ShortcodeParser(string startDelimiter, string endDelimiter, IReadOnlyShortcodeCollection shortcodes)
        {
            _startDelimiter = new Delimiter(startDelimiter, true);
            _endDelimiter = new Delimiter(endDelimiter, false);
            _shortcodes = shortcodes;
        }

        public List<ShortcodeInstance> Parse(Stream stream)
        {
            List<ShortcodeInstance> instances = new List<ShortcodeInstance>();

            CurrentTag currentTag = null;
            ShortcodeInstance shortcode = null;
            StringBuilder content = null;

            using (TextReader reader = new StreamReader(stream))
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
                                shortcode = GetShortcodeInstance(
                                    currentTag.FirstIndex,
                                    currentTag.Content.ToString(0, currentTag.Content.Length - _endDelimiter.Text.Length - 1));
                                shortcode.Finish(i);
                                instances.Add(shortcode);
                                shortcode = null;
                            }
                            else
                            {
                                // Look for a closing tag
                                shortcode = GetShortcodeInstance(
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
                                shortcode.Content = content.ToString(0, content.Length - _startDelimiter.Text.Length - 1);
                                shortcode.Finish(i);
                                instances.Add(shortcode);

                                shortcode = null;
                                content = null;
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

            return instances;
        }

        private ShortcodeInstance GetShortcodeInstance(int firstIndex, string tagContent)
        {
            // Split the tag content into name and arguments
            IEnumerable<string> split = ArgumentSplitter.Split(tagContent);
            string name = split.FirstOrDefault();
            if (name == null)
            {
                throw new ShortcodeParserException("Shortcode must have a name");
            }
            string[] arguments = split.Skip(1).ToArray();

            // Try to get the shortcode
            if (!_shortcodes.Contains(name))
            {
                throw new ShortcodeParserException($"A shortcode with the name {name} was not found");
            }

            return new ShortcodeInstance(firstIndex, name, arguments);
        }
    }
}
