using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wyam
{
    /// <summary>
    /// Parses INI-like args in key=value format.
    /// </summary>
    internal static class MetadataParser
    {
        public static IReadOnlyDictionary<string, object> Parse(IEnumerable<string> args) =>
            args.Select(ParsePair).ToDictionary(pair => pair.Key, pair => pair.Value);

        public static KeyValuePair<string, object> ParsePair(string arg)
        {
            // Find the first unescaped equal
            for (int i = 0; i < arg.Length; i++)
            {
                if (arg[i] == '=' && i > 0 && arg[i - 1] != '\\')
                {
                    return new KeyValuePair<string, object>(
                        Unescape(arg.Substring(0, i).Trim()),
                        ProcessValue(arg.Substring(i + 1).Trim()));
                }
            }

            // If one wasn't found, just unescape the whole string and set a null value
            return new KeyValuePair<string, object>(Unescape(arg.Trim()), null);
        }

        // Checks for arrays
        public static object ProcessValue(string s)
        {
            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                // Array, split on commas
                List<string> items = s.Substring(1, s.Length - 2).Split(',').ToList();

                // Trim and merge values
                List<string> values = new List<string>();
                int c = 0;
                while (c < items.Count)
                {
                    string value = items[c];

                    // Deal with comma escapes
                    int end = c;
                    while (items[end].EndsWith("\\"))
                    {
                        end++;
                    }
                    if (end != c)
                    {
                        value = string.Join(",", items.Skip(c).Take(end - c + 1)).Replace("\\,", ",");
                        c = end;
                    }

                    // Add the value
                    values.Add(Unescape(value.Trim()));
                    c++;
                }
                return values.Cast<object>().ToArray();
            }

            // Not an array, just unescape the value
            return Unescape(s);
        }

        // Based on code from StackOverflow: http://stackoverflow.com/a/25471811/807064
        public static string Unescape(string s)
        {
            StringBuilder sb = new StringBuilder();
            Regex r = new Regex("\\\\[=abfnrtv?\"'\\\\]|\\\\[0-3]?[0-7]{1,2}|\\\\u[0-9a-fA-F]{4}|\\\\U[0-9a-fA-F]{8}|.");
            MatchCollection mc = r.Matches(s, 0);

            foreach (Match m in mc)
            {
                if (m.Length == 1)
                {
                    sb.Append(m.Value);
                }
                else
                {
                    if (m.Value[1] >= '0' && m.Value[1] <= '7')
                    {
                        int i = Convert.ToInt32(m.Value.Substring(1), 8);
                        sb.Append((char)i);
                    }
                    else if (m.Value[1] == 'u')
                    {
                        int i = Convert.ToInt32(m.Value.Substring(2), 16);
                        sb.Append((char)i);
                    }
                    else if (m.Value[1] == 'U')
                    {
                        int i = Convert.ToInt32(m.Value.Substring(2), 16);
                        sb.Append(char.ConvertFromUtf32(i));
                    }
                    else
                    {
                        switch (m.Value[1])
                        {
                            case 'a':
                                sb.Append('\a');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'v':
                                sb.Append('\v');
                                break;
                            case '=':
                                sb.Append('=');
                                break;
                            default:
                                sb.Append(m.Value[1]);
                                break;
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
