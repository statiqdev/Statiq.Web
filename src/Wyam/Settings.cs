using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Configuration.Preprocessing;

namespace Wyam
{
    internal class Settings
    {
        public bool HelpDirectives = false;
        public bool Watch = false;
        public bool NoClean = false;
        public bool NoCache = false;
        public bool Preview = false;
        public int PreviewPort = 5080;
        public bool PreviewForceExtension = false;
        public FilePath LogFilePath = null;
        public bool Verbose = false;
        public bool Attach = false;
        public bool UpdatePackages = false;
        public bool UseLocalPackages = false;
        public bool UseGlobalSources = false;
        public DirectoryPath PackagesPath = null;
        public bool OutputScript = false;
        public bool VerifyConfig = false;
        public string Stdin = null;
        public DirectoryPath RootPath = null;
        public IReadOnlyList<DirectoryPath> InputPaths = null;
        public DirectoryPath OutputPath = null;
        public DirectoryPath PreviewRoot = null;
        public FilePath ConfigFilePath = null;
        public IReadOnlyDictionary<string, object> GlobalMetadata = null;
        public IReadOnlyDictionary<string, object> InitialMetadata = null;

        public bool ParseArgs(string[] args, Preprocessor preprocessor, out bool hasErrors)
        {
            System.CommandLine.ArgumentSyntax parsed = System.CommandLine.ArgumentSyntax.Parse(args, syntax =>
            {
                // Any changes here should also be made in Cake.Wyam

                if (syntax.DefineOption("help-directives", ref HelpDirectives, "Displays help for the various preprocessor directives.").IsSpecified)
                {
                    // Don't care about anything else
                    return;
                }
                syntax.DefineOption("w|watch", ref Watch, "Watches the input folder for any changes.");
                Preview = syntax.DefineOption("p|preview", ref PreviewPort, false, "Start the preview web server on the specified port (default is " + PreviewPort + ").").IsSpecified;
                if (syntax.DefineOption("force-ext", ref PreviewForceExtension, "Force the use of extensions in the preview web server (by default, extensionless URLs may be used).").IsSpecified && !Preview)
                {
                    syntax.ReportError("force-ext can only be specified if the preview server is running.");
                }
                if (syntax.DefineOption("preview-root", ref PreviewRoot, DirectoryPath.FromString, "The path to the root of the preview server, if not the output folder.").IsSpecified && !Preview)
                {
                    syntax.ReportError("preview-root can only be specified if the preview server is running.");
                }
                syntax.DefineOptionList("i|input", ref InputPaths, DirectoryPath.FromString, "The path(s) of input files, can be absolute or relative to the current folder.");
                syntax.DefineOption("o|output", ref OutputPath, DirectoryPath.FromString, "The path to output files, can be absolute or relative to the current folder.");
                syntax.DefineOption("c|config", ref ConfigFilePath, FilePath.FromString, "Configuration file (by default, config.wyam is used).");
                syntax.DefineOption("u|update-packages", ref UpdatePackages, "Check the NuGet server for more recent versions of each package and update them if applicable.");
                syntax.DefineOption("use-local-packages", ref UseLocalPackages, "Toggles the use of a local NuGet packages folder.");
                syntax.DefineOption("use-global-sources", ref UseGlobalSources, "Toggles the use of the global NuGet sources (default is false).");
                syntax.DefineOption("packages-path", ref PackagesPath, DirectoryPath.FromString, "The packages path to use (only if use-local is true).");
                syntax.DefineOption("output-script", ref OutputScript, "Outputs the config script after it's been processed for further debugging.");
                syntax.DefineOption("verify-config", ref VerifyConfig, false, "Compile the configuration but do not execute.");
                syntax.DefineOption("noclean", ref NoClean, "Prevents cleaning of the output path on each execution.");
                syntax.DefineOption("nocache", ref NoCache, "Prevents caching information during execution (less memory usage but slower execution).");
                syntax.DefineOption("v|verbose", ref Verbose, "Turns on verbose output showing additional trace message useful for debugging.");
                syntax.DefineOption("attach", ref Attach, "Pause execution at the start of the program until a debugger is attached.");

                LogFilePath = $"wyam-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
                if (!syntax.DefineOption("l|log", ref LogFilePath, FilePath.FromString, false, "Log all trace messages to the specified log file (by default, wyam-[datetime].txt).").IsSpecified)
                {
                    LogFilePath = null;
                }

                // Metadata
                IReadOnlyList<string> globalMetadata = null;
                if (syntax.DefineOptionList("g|global", ref globalMetadata, "Specifies global metadata as a sequence of key=value pairs.").IsSpecified)
                {
                    GlobalMetadata = MetadataParser.Parse(globalMetadata);
                }
                IReadOnlyList<string> initialMetadata = null;
                if (syntax.DefineOptionList("initial", ref initialMetadata, "Specifies initial document metadata as a sequence of key=value pairs.").IsSpecified)
                {
                    InitialMetadata = MetadataParser.Parse(initialMetadata);
                }

                // Directives
                foreach (IDirective directive in preprocessor.Directives.Where(x => x.SupportsCli))
                {
                    // Get the option name and help text
                    string optionName = (string.IsNullOrEmpty(directive.ShortName) ? string.Empty : directive.ShortName + "|") + directive.Name;
                    string optionHelp = $"{directive.Description}{(string.IsNullOrEmpty(directive.GetHelpText()) ? string.Empty : " See below for syntax details.")}";

                    // Single or multiple?
                    if (directive.SupportsMultiple)
                    {
                        IReadOnlyList<string> directiveValues = null;
                        syntax.DefineOptionList(optionName, ref directiveValues, optionHelp);
                        if (directiveValues != null)
                        {
                            foreach (string directiveValue in directiveValues)
                            {
                                preprocessor.AddValue(new DirectiveValue(directive.Name, directiveValue));
                            }
                        }
                    }
                    else
                    {
                        string directiveValue = null;
                        syntax.DefineOption(optionName, ref directiveValue, optionHelp);
                        if (directiveValue != null)
                        {
                            preprocessor.AddValue(new DirectiveValue(directive.Name, directiveValue));
                        }
                    }
                }

                // Root
                if (syntax.DefineParameter("root", ref RootPath, DirectoryPath.FromString, "The folder (or config file) to use.").IsSpecified)
                {
                    // If a root folder was defined, but it actually points to a file, set the root folder to the directory
                    // and use the specified file as the config file (if a config file was already specified, it's an error)
                    FilePath rootDirectoryPathAsConfigFile = new DirectoryPath(Environment.CurrentDirectory).CombineFile(RootPath.FullPath);
                    if (File.Exists(rootDirectoryPathAsConfigFile.FullPath))
                    {
                        // The specified root actually points to a file...
                        if (ConfigFilePath != null)
                        {
                            syntax.ReportError("A config file was both explicitly specified and specified in the root folder.");
                        }
                        else
                        {
                            ConfigFilePath = rootDirectoryPathAsConfigFile.FileName;
                            RootPath = rootDirectoryPathAsConfigFile.Directory;
                        }
                    }
                }
            });

            hasErrors = parsed.HasErrors;

            // Set verbose tracing
            if (Verbose)
            {
                Trace.Level = System.Diagnostics.SourceLevels.Verbose;
            }

            Stdin = StandardInputReader.Read();
            
            if (parsed.IsHelpRequested())
            {
                foreach (IDirective directive in preprocessor.Directives.Where(x => x.SupportsCli))
                {
                    string helpText = directive.GetHelpText();
                    if(!string.IsNullOrEmpty(helpText))
                    {
                        Console.WriteLine($"--{directive.Name} usage:");
                        Console.WriteLine();
                        Console.WriteLine(directive.GetHelpText());
                        Console.WriteLine();
                    }
                }
            }

            return !(parsed.IsHelpRequested() || hasErrors);
        }
    }
}