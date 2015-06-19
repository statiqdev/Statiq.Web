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
        // ReadFile/WriteFiles/CopyFiles
        public const string SourceFileRoot = "SourceFileRoot";
        public const string SourceFileBase = "SourceFileBase";
        public const string SourceFileExt = "SourceFileExt";
        public const string SourceFileName = "SourceFileName";
        public const string SourceFileDir = "SourceFileDir";
        public const string SourceFilePath = "SourceFilePath";
        public const string RelativeFilePath = "RelativeFilePath";
        public const string DestinationFilePath = "DestinationFilePath";
    }
}
