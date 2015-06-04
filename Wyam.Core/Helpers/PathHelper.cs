using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Helpers
{
    // This contains some additional helpers for the file system
    public class PathHelper
    {
        // From http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        public static String GetRelativePath(String fromPath, String toPath)
        {
            if (fromPath == null)
            {
                throw new ArgumentNullException("fromPath");
            }

            if (toPath == null)
            { 
                throw new ArgumentNullException("toPath");
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

            // find common root
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

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                {
                    relativePath.Add("..");
                }
            }

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            // create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;
        }
    }
}
