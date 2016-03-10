using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Globbing
{
    public static class Globber
    {
        // Initially based on code from Reliak.FileSystemGlobbingExtensions (https://github.com/reliak/Reliak.FileSystemGlobbingExtensions)
        public static IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns)
        {
            Matcher matcher = new Matcher(StringComparison.Ordinal);

            // Expand braces
            IEnumerable<string> expandedPatterns = patterns
                .SelectMany(ExpandBraces)
                .Select(f => f.Replace("\\{", "{").Replace("\\}", "}")); // Normalize escaped braces

            // Add the patterns, any that start with ! are exclusions
            foreach (string expandedPattern in expandedPatterns)
            {
                var isExclude = expandedPattern[0] == '!';
                var finalPattern = isExclude ? expandedPattern.Substring(1) : expandedPattern;

                // No support for absolute paths
                if (System.IO.Path.IsPathRooted(finalPattern))
                {
                    throw new ArgumentException($"Rooted globbing patterns are not supported ({expandedPattern})", nameof(patterns));
                }

                // Add exclude or include pattern to matcher
                if (isExclude)
                {
                    matcher.AddExclude(finalPattern);
                }
                else
                {
                    matcher.AddInclude(finalPattern);
                }
            }

            DirectoryInfoBase directoryInfo = new DirectoryInfo(directory);
            PatternMatchingResult result = matcher.Execute(directoryInfo);
            return result.Files.Select(match => directory.GetFile(match.Path));
        }


        private static readonly Regex HasBraces = new Regex(@"\{.*\}");
        private static readonly Regex NumericSet = new Regex(@"^\{(-?[0-9]+)\.\.(-?[0-9]+)\}");

        // Initially based on code from Minimatch (https://github.com/SLaks/Minimatch/blob/master/Minimatch/Minimatcher.cs)
        // Brace expansion:
        // a{b,c}d -> abd acd
        // a{b,}c -> abc ac
        // a{0..3}d -> a0d a1d a2d a3d
        // a{b,c{d,e}f}g -> abg acdfg acefg
        // a{b,c}d{e,f}g -> abdeg acdeg abdeg abdfg
        //
        // Invalid sets are not expanded.
        // a{2..}b -> a{2..}b
        // a{b}c -> a{b}c
        ///<summary>Expands all brace ranges in a pattern, returning a sequence containing every possible combination.</summary>
        public static IEnumerable<string> ExpandBraces(string pattern)
        {
            if (!HasBraces.IsMatch(pattern))
            {
                // shortcut. no need to expand.
                return new[] { pattern };
            }

            bool escaping = false;
            int i;
            // examples and comments refer to this crazy pattern:
            // a{b,c{d,e},{f,g}h}x{y,z}
            // expected:
            // abxy
            // abxz
            // acdxy
            // acdxz
            // acexy
            // acexz
            // afhxy
            // afhxz
            // aghxy
            // aghxz

            // everything before the first \{ is just a prefix.
            // So, we pluck that off, and work with the rest,
            // and then prepend it to everything we find.
            if (pattern[0] != '{')
            {
                // console.error(pattern)
                string prefix = null;
                for (i = 0; i < pattern.Length; i++)
                {
                    char c = pattern[i];
                    // console.error(i, c)
                    if (c == '\\')
                    {
                        escaping = !escaping;
                    }
                    else if (c == '{' && !escaping)
                    {
                        prefix = pattern.Substring(0, i);
                        break;
                    }
                }

                // actually no sets, all { were escaped.
                if (prefix == null)
                {
                    // console.error("no sets")
                    return new[] { pattern };
                }

                return ExpandBraces(pattern.Substring(i)).Select(t => prefix + t);
            }

            // now we have something like:
            // {b,c{d,e},{f,g}h}x{y,z}
            // walk through the set, expanding each part, until
            // the set ends.  then, we'll expand the suffix.
            // If the set only has a single member, then'll put the {} back

            // first, handle numeric sets, since they're easier
            Match numset = NumericSet.Match(pattern);
            if (numset.Success)
            {
                // console.error("numset", numset[1], numset[2])
                List<string> suf = ExpandBraces(pattern.Substring(numset.Length)).ToList();
                int start = int.Parse(numset.Groups[1].Value),
                end = int.Parse(numset.Groups[2].Value),
                inc = start > end ? -1 : 1;
                List<string> retVal = new List<string>();
                for (int w = start; w != (end + inc); w += inc)
                {
                    // append all the suffixes
                    retVal.AddRange(suf.Select(t => w + t));
                }
                return retVal;
            }

            // ok, walk through the set
            // We hope, somewhat optimistically, that there
            // will be a } at the end.
            // If the closing brace isn't found, then the pattern is
            // interpreted as braceExpand("\\" + pattern) so that
            // the leading \{ will be interpreted literally.
            int depth = 1;
            List<string> set = new List<string>();
            string member = "";

            for (i = 1 /* skip the \{ */ ; i < pattern.Length && depth > 0; i++) 
            {
                char c = pattern[i];
                // console.error("", i, c)

                if (escaping)
                {
                    escaping = false;
                    member += "\\" + c;
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                            escaping = true;
                            continue;

                        case '{':
                            depth++;
                            member += "{";
                            continue;

                        case '}':
                            depth--;
                            // if this closes the actual set, then we're done
                            if (depth == 0)
                            {
                                set.Add(member);
                                member = "";
                                // pluck off the close-brace
                                break;
                            }
                            else
                            {
                                member += c;
                                continue;
                            }

                        case ',':
                            if (depth == 1)
                            {
                                set.Add(member);
                                member = "";
                            }
                            else
                            {
                                member += c;
                            }
                            continue;

                        default:
                            member += c;
                            continue;
                    } // switch
                } // else
            } // for

            // now we've either finished the set, and the suffix is
            // pattern.substr(i), or we have *not* closed the set,
            // and need to escape the leading brace
            if (depth != 0)
            {
                // console.error("didn't close", pattern)
                return ExpandBraces("\\" + pattern);
            }

            // ["b", "c{d,e}","{f,g}h"] ->
            //   ["b", "cd", "ce", "fh", "gh"]
            bool addBraces = set.Count == 1;

            set = set.SelectMany(p => ExpandBraces(p)).ToList();

            if (addBraces)
            {
                set = set.Select(s => "{" + s + "}").ToList();
            }

            // now attach the suffixes.
            // x{y,z} -> ["xy", "xz"]
            // console.error("set", set)
            // console.error("suffix", pattern.substr(i))
            return ExpandBraces(pattern.Substring(i)).SelectMany(s1 => set.Select(s2 => s2 + s1));
        }
    }
}
