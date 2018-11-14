using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Wyam.Common.IO;
using Wyam.Configuration.Preprocessing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Commands
{
    /// <summary>
    /// Represents a single command line command, or a set of options and parameters.
    /// </summary>
    internal abstract class Command
    {
        private bool _attach = false;
        private bool _verbose = false;

        public abstract string Description { get; }

        public virtual string[] SupportedDirectives => null;

        protected virtual bool GlobalArguments => true;

        public void Parse(ArgumentSyntax syntax, Preprocessor preprocessor)
        {
            // Global options
            if (GlobalArguments)
            {
                syntax.DefineOption("v|verbose", ref _verbose, "Turns on verbose output showing additional trace message useful for debugging.");
                syntax.DefineOption("attach", ref _attach, "Pause execution at the start of the program until a debugger is attached.");
            }

            // Command options
            ParseOptions(syntax);

            // Directives
            if (SupportedDirectives != null)
            {
                foreach (IDirective directive in preprocessor.Directives
                    .Where(x => SupportedDirectives.Contains(x.Name, StringComparer.OrdinalIgnoreCase)))
                {
                    // Get the option name and help text
                    string optionName = (string.IsNullOrEmpty(directive.ShortName) ? string.Empty : directive.ShortName + "|") + directive.Name;
                    string optionHelp = $"{directive.Description}{(string.IsNullOrEmpty(directive.GetHelpText()) ? string.Empty : " See below for syntax details.")}";

                    // Single or multiple?
                    if (directive.SupportsMultiple)
                    {
                        // Multiple
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
                        // Single
                        string directiveValue = null;
                        syntax.DefineOption(optionName, ref directiveValue, optionHelp);
                        if (directiveValue != null)
                        {
                            preprocessor.AddValue(new DirectiveValue(directive.Name, directiveValue));
                        }
                    }
                }
            }

            // Command parameters
            ParseParameters(syntax);
        }

        protected virtual void ParseOptions(ArgumentSyntax syntax)
        {
        }

        protected virtual void ParseParameters(ArgumentSyntax syntax)
        {
        }

        public ExitCode Run(Preprocessor preprocessor)
        {
            if (GlobalArguments)
            {
                // Set verbose tracing
                if (_verbose)
                {
                    Trace.Level = System.Diagnostics.SourceLevels.Verbose;
                }

                // Attach
                if (_attach)
                {
                    Trace.Information($"Waiting for a debugger to attach to process {Process.GetCurrentProcess().Id} (or press a key to continue)...");
                    while (!Debugger.IsAttached && !Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                    }
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        Trace.Information("Key pressed, continuing execution");
                    }
                    else
                    {
                        Trace.Information("Debugger attached, continuing execution");
                    }
                }
            }

            return RunCommand(preprocessor);
        }

        protected abstract ExitCode RunCommand(Preprocessor preprocessor);

        // Used by both build and new
        protected static void ParseRootPathParameter(ArgumentSyntax syntax, ConfigOptions configOptions)
        {
            if (syntax.DefineParameter("root", ref configOptions.RootPath, DirectoryPathFromArg, "The folder (or config file) to use.").IsSpecified)
            {
                // If a root folder was defined, but it actually points to a file, set the root folder to the directory
                // and use the specified file as the config file (if a config file was already specified, it's an error)
                FilePath rootDirectoryPathAsConfigFile = new DirectoryPath(Environment.CurrentDirectory).CombineFile(configOptions.RootPath.FullPath);
                if (System.IO.File.Exists(rootDirectoryPathAsConfigFile.FullPath))
                {
                    // The specified root actually points to a file...
                    if (configOptions.ConfigFilePath != null)
                    {
                        syntax.ReportError("A config file was both explicitly specified and specified in the root folder.");
                    }
                    else
                    {
                        configOptions.ConfigFilePath = rootDirectoryPathAsConfigFile.FileName;
                        configOptions.RootPath = rootDirectoryPathAsConfigFile.Directory;
                    }
                }
            }
        }

        /// <summary>
        /// This helper method is needed due to the escape behavior of command line argument parsing and the
        /// way PowerShell handles tab completion of directory paths. See https://github.com/Wyamio/Wyam/issues/472#issuecomment-286884482
        /// </summary>
        /// <param name="arg">The command line argument containing the path</param>
        /// <returns>A <see cref="DirectoryPath"/></returns>
        protected static DirectoryPath DirectoryPathFromArg(string arg) => DirectoryPath.FromString(arg.TrimEnd(new[] { '"', '\'' }));
    }
}