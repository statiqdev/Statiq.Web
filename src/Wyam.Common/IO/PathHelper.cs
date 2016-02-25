using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wyam.Common.IO
{
    // This contains some additional helpers for the file system
    [Obsolete("This will be replaced by new IO functionality in the next release")]
    public class PathHelper
    {
        private static readonly char[] WildcardCharacters = { '*', '?' };

        // Converts slashes to Path.DirectorySeparatorChar
        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string NormalizePath(string path)
        {
            return path
                .Replace('\\', System.IO.Path.DirectorySeparatorChar)
                .Replace('/', System.IO.Path.DirectorySeparatorChar);
        }

        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string ToLink(string path)
        {
            return path.Replace("\\", "/");
        }

        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string ToRootLink(string path)
        {
            return path.StartsWith("/") ? ToLink(path) : "/" + ToLink(path);
        }

        // From http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string GetRelativePath(string fromPath, string toPath)
        {
            // Remove single . self-references
            fromPath = fromPath.Replace(@"\.\", @"\");
            fromPath = fromPath.Trim('.');
            toPath = toPath.Replace(@"\.\", @"\");
            toPath = toPath.Trim('.');

            if (fromPath == null)
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (toPath == null)
            { 
                throw new ArgumentNullException(nameof(toPath));
            }
            
            // Check if both paths are rooted
            if (System.IO.Path.IsPathRooted(fromPath) && System.IO.Path.IsPathRooted(toPath))
            {
                // Check if it's a different root
                if (String.Compare(System.IO.Path.GetPathRoot(fromPath), System.IO.Path.GetPathRoot(toPath), StringComparison.Ordinal) != 0)
                {
                    return toPath;
                }
            }

            List<string> relativePath = new List<string>();
            string[] fromDirectories = fromPath.Split(System.IO.Path.DirectorySeparatorChar);

            string[] toDirectories = toPath.Split(System.IO.Path.DirectorySeparatorChar);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);

            int lastCommonRoot = -1;

            // Find common root
            for (int x = 0; x < length; x++)
            {
                if (String.Compare(fromDirectories[x], toDirectories[x], StringComparison.Ordinal) != 0)
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return toPath;
            }

            // Add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                {
                    relativePath.Add("..");
                }
            }

            // Add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            // Create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;
        }

        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string RemoveExtension(string path)
        {
            string extension = System.IO.Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(extension) ? path : path.Substring(0, path.Length - extension.Length);
        }

        [Obsolete("This will be replaced by new IO functionality in the next release")]
        public static string CombineToFullPath(params string[] paths)
        {
            string combinedPath = System.IO.Path.Combine(paths);
            int wildCardCharIndex = combinedPath.IndexOfAny(WildcardCharacters);

            if( wildCardCharIndex < 0 )
            {
                return System.IO.Path.GetFullPath(combinedPath);
            }

            string pathTillFirstWildCard = combinedPath.Substring(0, wildCardCharIndex);

            return System.IO.Path.GetFullPath(pathTillFirstWildCard) + combinedPath.Substring(wildCardCharIndex);
        }
    }
}
