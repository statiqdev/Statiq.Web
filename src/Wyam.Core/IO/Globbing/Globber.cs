using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Globbing
{
    public static class Globber
    {
        public static IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns)
        {
            Matcher matcher = new Matcher(StringComparison.Ordinal);

            // Add the patterns, any that start with ! are exclusions
            foreach (string pattern in patterns)
            {
                if (pattern[0] == '!')
                {
                    matcher.AddExclude(pattern.Substring(1));
                }
                else
                {
                    matcher.AddInclude(pattern);
                }
            }

            DirectoryInfoBase directoryInfo = new DirectoryInfo(directory);
            PatternMatchingResult result = matcher.Execute(directoryInfo);
            return result.Files.Select(match => directory.GetFile(match.Path));
        }
    }
}
