using System;
using Cake.Core;
using Cake.Core.IO;

namespace Cake.Wyam
{
    using System.Collections.Generic;
    using Cake.Core.Tooling;

    /// <summary>
    /// The Wyam Runner used to execute the Wyam Executable
    /// </summary>
    public sealed class WyamRunner : Tool<WyamSettings>
    {
        private readonly ICakeEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="WyamRunner" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="globber">The globber.</param>
        public WyamRunner(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IGlobber globber)
            : base(fileSystem, environment, processRunner, globber)
        {
            _environment = environment;
        }

        /// <summary>
        /// Publishes a Vsce package from the provided settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void Run(WyamSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            Run(settings, GetArguments(settings), new ProcessSettings(), null);
        }

        /// <summary>
        /// Gets the name of the tool.
        /// </summary>
        /// <returns>The name of the tool.</returns>
        protected override string GetToolName()
        {
            return "Wyam";
        }

        /// <summary>
        /// Gets the possible names of the tool executable.
        /// </summary>
        /// <returns>The tool executable name.</returns>
        protected override IEnumerable<string> GetToolExecutableNames()
        {
            return new[] { "Wyam.exe" };
        }

        private ProcessArgumentBuilder GetArguments(WyamSettings settings)
        {
            var builder = new ProcessArgumentBuilder();

            if (settings.ConfigurationFile != null)
            {
                builder.Append("--config");
                builder.AppendQuoted(settings.ConfigurationFile.MakeAbsolute(_environment).FullPath);
            }

            if (settings.InputDirectory != null)
            {
                builder.Append("--input");
                builder.AppendQuoted(settings.InputDirectory.MakeAbsolute(_environment).FullPath);
            }

            if (settings.OutputDirectory != null)
            {
                builder.Append("--output");
                builder.AppendQuoted(settings.OutputDirectory.MakeAbsolute(_environment).FullPath);
            }

            if (settings.NoClean)
            {
                builder.Append("--noclean");
            }

            if (settings.NoCache)
            {
                builder.Append("--nocache");
            }

            if (settings.UpdatePackages)
            {
                builder.Append("--update-packages");
            }

            if (settings.Watch)
            {
                builder.Append("--watch");
            }

            if (settings.Preview)
            {
                builder.Append("--preview");

                if (settings.PreviewPort != 0)
                {
                    builder.Append(settings.PreviewPort.ToString());
                }

                if (settings.PreviewForceExtensions)
                {
                    builder.Append("force-ext");
                }
            }

            if (settings.OutputScripts)
            {
                builder.Append("--output-scripts");
            }

            if (settings.LogFilePath != null)
            {
                builder.Append("--log");
                builder.AppendQuoted(settings.LogFilePath.MakeAbsolute(_environment).FullPath);
            }

            if (settings.Verbose)
            {
                builder.Append("--verbose");
            }

            if (settings.Pause)
            {
                builder.Append("--pause");
            }

            return builder;
        }
    }
}