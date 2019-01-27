using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.FileProviders.Local
{
    internal class LocalCaseSensitivityChecker
    {
        private readonly ConcurrentDictionary<string, bool> _cache = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Determines if the file system rooted at a given directory is case insensitive.
        /// Checks if the file system thinks the directory exists for both upper and lower case variants.
        /// If the directory doesn't exist in the first place, or it's entirely non-alpha, a fallback approach
        /// is used that creates a temporary file in the system's temporary file location and checks that.
        /// </summary>
        /// <param name="directory">The directory to check.</param>
        /// <returns><c>true</c> if the file system rooted at the directory is case sensitive, <c>false</c> otherwise.</returns>
        public bool IsCaseSensitive(LocalDirectory directory) =>
            _cache.GetOrAdd(directory.Path.FullPath, _ => IsDirectoryCaseSensitive(directory) ?? TempFileIsCaseSensitive.Value);

        // Returns null if the directory name couldn't be checked because it's entirely non-alpha
        // Also returns null if the directory doesn't exist in the first place
        private bool? IsDirectoryCaseSensitive(IDirectory directory)
        {
            string path = directory.Path.FullPath;
            if (!directory.Exists || !path.Any(c => char.IsLetter(c)))
            {
                return null;
            }
            return !(Directory.Exists(path.ToLower()) && Directory.Exists(path.ToUpper()));
        }

        private static readonly Lazy<bool> TempFileIsCaseSensitive = new Lazy<bool>(() =>
        {
            // Based on https://stackoverflow.com/questions/430256/how-do-i-determine-whether-the-filesystem-is-case-sensitive-in-net
            string file = null;
            try
            {
                file = Path.GetTempPath() + Guid.NewGuid().ToString().ToLower();
                File.CreateText(file).Close();
                return !File.Exists(file.ToUpper());
            }
            finally
            {
                if (file != null)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        });
    }
}
