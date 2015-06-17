using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Abstractions
{
    // Not an abstraction, but makes sense to put it here so other libraries can use it
    public static class MetadataKeys
    {
        // ReadFiles
        public const string FileRoot = "FileRoot";
        public const string FileBase = "FileBase";
        public const string FileExt = "FileExt";
        public const string FileName = "FileName";
        public const string FileDir = "FileDir";
        public const string FilePath = "FilePath";
        public const string FileRelative = "FileRelative";

        // ReadFile/WriteFiles/CopyFiles
        public const string SourcePath = "SourcePath";
        public const string DestinationPath = "DestinationPath";
    }
}
