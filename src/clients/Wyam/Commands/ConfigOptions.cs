using System.Collections.Generic;
using Wyam.Common.IO;

namespace Wyam.Commands
{
    internal class ConfigOptions
    {
#pragma warning disable SA1401 // Fields must be private
        public bool NoClean = false;
        public bool NoCache = false;
        public bool UpdatePackages = false;
        public bool UseLocalPackages = false;
        public bool UseGlobalSources = false;
        public DirectoryPath PackagesPath = null;
        public bool OutputScript = false;
        public bool IgnoreConfigHash = false;
        public bool NoOutputConfigAssembly = false;
        public string Stdin = null;
        public DirectoryPath RootPath = null;
        public IReadOnlyList<DirectoryPath> InputPaths = null;
        public DirectoryPath OutputPath = null;
        public FilePath ConfigFilePath = null;
        public IReadOnlyDictionary<string, object> Settings = null;
#pragma warning restore SA1401 // Fields must be private
    }
}