using System;
using System.Collections.Generic;
using System.IO;

namespace Wyam.Common.IO
{
    // This contains some additional helpers for the file system
    public class PathHelper
    {
        // Converts slashes to Path.DirectorySeparatorChar
        public static string NormalizePath(string path)
        {
            return path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);
        }

        public static string ToLink(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string ToRootLink(string path)
        {
            return path.StartsWith("/") ? ToLink(path) : "/" + ToLink(path);
        }

        // From http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (fromPath == null)
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (toPath == null)
            { 
                throw new ArgumentNullException(nameof(toPath));
            }
            
            // Check if both paths are rooted
            if (Path.IsPathRooted(fromPath) && Path.IsPathRooted(toPath))
            {
                // Check if it's a different root
                if (String.Compare(Path.GetPathRoot(fromPath), Path.GetPathRoot(toPath), StringComparison.Ordinal) != 0)
                {
                    return toPath;
                }
            }

            List<string> relativePath = new List<string>();
            string[] fromDirectories = fromPath.Split(Path.DirectorySeparatorChar);

            string[] toDirectories = toPath.Split(Path.DirectorySeparatorChar);

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

            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;
        }

        public static string RemoveExtension(string path)
        {
            string extension = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(extension) ? path : path.Substring(0, path.Length - extension.Length);
        }
    }
}
