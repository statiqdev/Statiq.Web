using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // Other modules outside Core might refer to these as strings, so make sure to do a text search if renaming
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
        public const string WriteExtension = "WriteExtension";
        public const string WritePath = "WritePath";
    }
}
