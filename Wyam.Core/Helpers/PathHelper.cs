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
            if (String.IsNullOrEmpty(fromPath))
                throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) 
                throw new ArgumentNullException("toPath");

            if (fromPath.Last() != Path.DirectorySeparatorChar) 
            { 
                fromPath += Path.DirectorySeparatorChar; 
            } 
            if (toPath.Last() != Path.DirectorySeparatorChar) 
            {
                toPath += Path.DirectorySeparatorChar;
            }

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

    }
}
