using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Wyam
{
    /// <summary>
    /// Contains settings used by <see cref="WyamRunner"/>.
    /// </summary>
    public sealed class WyamSettings : ToolSettings
    {
        /// <summary>
        /// Gets or sets a value indicating the Configuration File that should be used while running Wyam.
        /// </summary>
        public FilePath ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Input Directory that should be used while running Wyam.
        /// </summary>
        public DirectoryPath InputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the Output Directory that should be used while running Wyam.
        /// </summary>
        public DirectoryPath OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to not clean output folder on every execution.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool NoClean { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to turn off Caching mechanism on all modules.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool NoCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable updating of packages.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool UpdatePackages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable watching of input folder for changes to files.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool Watch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable previewing of the generated content in built in web server.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool Preview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the port number to use for previewing.
        /// </summary>
        /// <remarks>Default is 5080</remarks>
        public int PreviewPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable forcing of using file extensions.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool PreviewForceExtensions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to output the scripts at end of execution.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool OutputScripts { get; set; }

        /// <summary>
        /// Gets or sets the path to the Wyam log file.
        /// </summary>
        public FilePath LogFilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run in verbose mode.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to pause execution at the start of the program.
        /// </summary>
        /// <remarks>Default is false</remarks>
        public bool Pause { get; set; }
    }
}