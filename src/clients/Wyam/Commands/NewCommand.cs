using System;
using System.CommandLine;
using System.Linq;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.Preprocessing;

namespace Wyam.Commands
{
    internal class NewCommand : Command
    {
        private readonly ConfigOptions _configOptions = new ConfigOptions();

        private DirectoryPath _inputPath = null;

        public override string Description => "Scaffolds the given recipe into a specified path.";

        public override string[] SupportedDirectives => new[]
        {
            "nuget",
            "nuget-source",
            "assembly",
            "recipe"
        };

        protected override void ParseOptions(ArgumentSyntax syntax)
        {
            syntax.DefineOption("u|update-packages", ref _configOptions.UpdatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.");
            syntax.DefineOption("use-local-packages", ref _configOptions.UseLocalPackages, "Toggles the use of a local NuGet packages folder.");
            syntax.DefineOption("use-global-sources", ref _configOptions.UseGlobalSources, "Toggles the use of the global NuGet sources (default is false).");
            syntax.DefineOption("packages-path", ref _configOptions.PackagesPath, DirectoryPath.FromString, "The packages path to use (only if use-local is true).");
            syntax.DefineOption("i|input", ref _inputPath, DirectoryPath.FromString, "The path of input files, can be absolute or relative to the current folder.");
            syntax.DefineOption("c|config", ref _configOptions.ConfigFilePath, FilePath.FromString, "Configuration file (by default, config.wyam is used).");
        }

        protected override void ParseParameters(ArgumentSyntax syntax)
        {
            ParseRootPathParameter(syntax, _configOptions);
        }

        protected override ExitCode RunCommand(Preprocessor preprocessor)
        {
            // Make sure we actually got a recipe value
            if (preprocessor.Values.All(x => x.Name != "recipe"))
            {
                Trace.Critical("A recipe must be specified");
                return ExitCode.CommandLineError;
            }

            // Fix the root folder and other files
            DirectoryPath currentDirectory = Environment.CurrentDirectory;
            _configOptions.RootPath = _configOptions.RootPath == null ? currentDirectory : currentDirectory.Combine(_configOptions.RootPath);
            _configOptions.ConfigFilePath = _configOptions.RootPath.CombineFile(_configOptions.ConfigFilePath ?? "config.wyam");
            _inputPath = _inputPath ?? "input";

            // Get the engine and configurator
            using (EngineManager engineManager = EngineManager.Get(preprocessor, _configOptions))
            {
                if (engineManager == null)
                {
                    return ExitCode.CommandLineError;
                }

                // Check to make sure the directory is empty (and provide option to clear it)
                IDirectory inputDirectory = engineManager.Engine.FileSystem.GetRootDirectory(_inputPath);
                if (inputDirectory.Exists)
                {
                    Console.WriteLine($"Input directory {inputDirectory.Path.FullPath} exists, are you sure you want to clear it [y|N]?");
                    char inputChar = Console.ReadKey(true).KeyChar;
                    if (inputChar != 'y' && inputChar != 'Y')
                    {
                        Trace.Information($"Input directory will not be cleared");
                        return ExitCode.Normal;
                    }
                    Trace.Information($"Input directory will be cleared");
                }
                else
                {
                    Trace.Information($"Input directory {inputDirectory.Path.FullPath} does not exist and will be created");
                }
                if (inputDirectory.Exists)
                {
                    inputDirectory.Delete(true);
                }
                inputDirectory.Create();

                // Check the config file (and provide option to clear it)
                IFile configFile = engineManager.Engine.FileSystem.GetRootFile(_configOptions.ConfigFilePath);
                if (configFile.Exists)
                {
                    Console.WriteLine($"Configuration file {configFile.Path.FullPath} exists, are you sure you want to potentially overwrite it [y|N]?");
                    char inputChar = Console.ReadKey(true).KeyChar;
                    if (inputChar != 'y' && inputChar != 'Y')
                    {
                        Trace.Information($"Configuration file will not be overwritten");
                        configFile = null;
                    }
                    else
                    {
                        Trace.Information($"Configuration file may be overwritten");
                    }
                }
                else
                {
                    Trace.Information($"Configuration file {configFile.Path.FullPath} does not exist and may be created");
                }

                // We can ignore theme packages since we don't care about the theme for scaffolding
                engineManager.Configurator.IgnoreKnownThemePackages = true;

                // Configure everything (primarily to get the recipe)
                try
                {
                    engineManager.Configurator.Configure(null);
                }
                catch (Exception ex)
                {
                    Trace.Critical("Error while configuring engine: {0}", ex.Message);
                    return ExitCode.ConfigurationError;
                }

                // Scaffold the recipe
                engineManager.Configurator.Recipe.Scaffold(configFile, inputDirectory);
            }

            return ExitCode.Normal;
        }
    }
}