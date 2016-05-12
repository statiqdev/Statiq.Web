using System.Collections.Generic;

namespace Cake.Wyam
{
    public class NuGetSettings
    {
        /// <summary>
        /// Specifies that prerelease packages are allowed.
        /// </summary>
        public bool Prerelease;

        /// <summary>
        /// Specifies that unlisted packages are allowed.
        /// </summary>
        /// 
        public bool Unlisted;

        /// <summary>
        /// Indicates that only the specified package source(s) should be used to find the package.
        /// </summary>
        public bool Exclusive;

        /// <summary>
        /// Specifies the version of the package to use.
        /// </summary>
        public string Version;

        /// <summary>
        /// Specifies the package source(s) to get the package from.
        /// </summary>
        public IEnumerable<string> Source;

        /// <summary>
        /// The package to install.
        /// </summary>
        public string Package;
    }
}