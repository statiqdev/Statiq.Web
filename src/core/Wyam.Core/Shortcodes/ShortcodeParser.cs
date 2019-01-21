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
        private readonly Delimiter _startDelimiter;
        private readonly Delimiter _endDelimiter;
        private readonly Dictionary<string, IShortcode> _shortcodes;

        public ShortcodeParser(string startDelimiter, string endDelimiter, Dictionary<string, IShortcode> shortcodes)
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
            int lastWhiteSpace = 0;
            int lastNonWhiteSpace = 0;
            char? style = null;
            ShortcodeInstance finishShortcode = null;
            bool readyToFinish = false;
            bool justFinished = false;
            using (TextReader reader = new StreamReader(stream))
            {
                int r;
                int i = 0;
                while ((r = reader.Read()) != -1)
                {
                    char c = (char)r;

                    // Track leading whitespace and chars
                    if (char.IsWhiteSpace(c))
                    {
                        if (lastWhiteSpace <= lastNonWhiteSpace)
                        {
                            lastWhiteSpace = i;
                        }
                    }
                    else
                    {
                        if (lastNonWhiteSpace <= lastWhiteSpace)
                        {
                            lastNonWhiteSpace = i;
                        }
                    }

                    // Look for delimiters and tags
                    if (currentTag == null && shortcode == null)
                    {
                        // Searching for open tag start delimiter
                        if (_startDelimiter.Locate(c, false, ref style))
                        {
                            currentTag = new CurrentTag(i - _startDelimiter.Text.Length, lastWhiteSpace, lastNonWhiteSpace);
                        }
                    }
                    else if (currentTag != null && shortcode == null)
                    {
                        // Searching for open tag end delimiter
                        currentTag.Content.Append(c);
                        if (_endDelimiter.Locate(c, false, ref style))
                        {
                            currentTag.Content.Remove(currentTag.Content.Length - _endDelimiter.Text.Length - 1, _endDelimiter.Text.Length + 1);

                            // Figure out the first index depending on tag style
                            int firstIndex = currentTag.FirstIndex;
                            if (style == '*')
                            {
                                firstIndex = currentTag.LastNonWhiteSpace;
                            }
                            else if (style == '#')
                            {
                                firstIndex = currentTag.LastWhiteSpace;
                            }

                            // Is this self-closing?
                            if (currentTag.Content[currentTag.Content.Length - 1] == '/')
                            {
                                // Self-closing
                                currentTag.Content.Remove(currentTag.Content.Length - 1, 1);
                                shortcode = GetShortcodeInstance(style.Value, firstIndex, currentTag.Content.ToString());
                                shortcode.Finish(i);
                                instances.Add(shortcode);
                                shortcode = null;
                                currentTag = null;
                            }
                            else
                            {
                                // Look for a closing tag
                                shortcode = GetShortcodeInstance(style.Value, firstIndex, currentTag.Content.ToString());
                                content = new StringBuilder();
                                currentTag = null;
                            }
                        }
                    }
                    else if (currentTag == null && shortcode != null)
                    {
                        content.Append(c);

                        // Searching for close tag start delimiter
                        if (_startDelimiter.Locate(c, true, ref style))
                        {
                            currentTag = new CurrentTag(i, lastWhiteSpace, lastNonWhiteSpace);
                        }
                    }
                    else
                    {
                        currentTag.Content.Append(c);

                        // Searching for close tag end delimiter
                        if (_endDelimiter.Locate(c, false, ref style))
                        {
                            // Get the name of this shortcode close tag
                            currentTag.Content.Remove(currentTag.Content.Length - _endDelimiter.Text.Length - 1, _endDelimiter.Text.Length + 1);
                            string name = new string(currentTag.Content.ToString().Trim().TakeWhile(x => !char.IsWhiteSpace(x)).ToArray());

                            // Make sure it's the name name
                            if (name.Equals(shortcode.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                shortcode.Content = content.Remove(content.Length - _startDelimiter.Text.Length - 2, _startDelimiter.Text.Length + 2).ToString();

                                // Do we need to keep scanning because style consumes after tag?
                                if (shortcode.Style == '*' || shortcode.Style == '#')
                                {
                                    finishShortcode = shortcode;
                                }
                                else
                                {
                                    shortcode.Finish(i);
                                    instances.Add(shortcode);
                                }
                            }

                            shortcode = null;
                            content = null;
                            currentTag = null;
                            style = null;
                            justFinished = true;  // Makes sure we skip the look ahead on the index where we just finished
                        }
                    }

                    // Finish up the previous shortcode if we need to
                    if (finishShortcode != null && !justFinished)
                    {
                        // Did we start another shortcode while we were waiting?
                        if (shortcode != null)
                        {
                            finishShortcode.Finish(shortcode.FirstIndex);
                            instances.Add(finishShortcode);
                            finishShortcode = null;
                            readyToFinish = false;
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            if (finishShortcode.Style == '*')
                            {
                                finishShortcode.Finish(i);
                                instances.Add(finishShortcode);
                                finishShortcode = null;
                                readyToFinish = false;
                            }

                            // Only once we get whitespace can we finish a # style on non-whitespace
                            readyToFinish = true;
                        }
                        else
                        {
                            if (finishShortcode.Style == '#')
                            {
                                if (readyToFinish)
                                {
                                    finishShortcode.Finish(i);
                                    instances.Add(finishShortcode);
                                    finishShortcode = null;
                                    readyToFinish = false;
                                }
                            }
                        }
                    }

                    justFinished = false;

                    i++;
                }
            }

            return instances;
        }

        private ShortcodeInstance GetShortcodeInstance(char style, int firstIndex, string tagContent)
        {
            // Split the tag content into name and arguments
            IEnumerable<string> split = ArgumentSplitter.Split(tagContent);
            string name = split.FirstOrDefault();
            if (name == null)
            {
                throw new ArgumentException("Shortcode must have a name");
            }
            string[] arguments = split.Skip(1).ToArray();

            // Try to get the shortcode
            if (!_shortcodes.TryGetValue(name, out IShortcode shortcode))
            {
                throw new ArgumentException($"A shortcode with the name {name} was not found");
            }

            return new ShortcodeInstance(style, firstIndex, name, arguments, shortcode);
        }
    }
}
