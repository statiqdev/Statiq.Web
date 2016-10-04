using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.StaticFiles;
using Owin;
using Wyam.Commands;
using Wyam.Common.IO;
using Wyam.Configuration.Preprocessing;
using Wyam.Owin;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam
{
    public class Program
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEvent;
            Program program = new Program();
            return program.Run(args);
        }

        private static void UnhandledExceptionEvent(object sender, UnhandledExceptionEventArgs e)
        {
            // Exit with a error exit code
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Trace.Critical(exception.Message);
                Trace.Verbose(exception.ToString());
            }
            Environment.Exit((int)ExitCode.UnhandledError);
        }

        private int Run(string[] args)
        {
            // Add a default trace listener
            Trace.AddListener(new SimpleColorConsoleTraceListener { TraceOutputOptions = System.Diagnostics.TraceOptions.None });
            
            // Output version info
            AssemblyInformationalVersionAttribute versionAttribute
                = Attribute.GetCustomAttribute(typeof(Program).Assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            Trace.Information("Wyam version {0}", versionAttribute == null ? "unknown" : versionAttribute.InformationalVersion);

            // It's not a serious console app unless there's some ASCII art
            OutputLogo();

            // Parse the command line
            Preprocessor preprocessor = new Preprocessor();
            Command command;
            try
            {
                bool hasParseArgsErrors;
                command = CommandParser.Parse(args, preprocessor, out hasParseArgsErrors);
                if (command == null)
                {
                    return hasParseArgsErrors ? (int)ExitCode.CommandLineError : (int)ExitCode.Normal;
                }
            }
            catch (Exception ex)
            {
                Trace.Error("Error while parsing command line: {0}", ex.Message);
                if (Trace.Level == SourceLevels.Verbose)
                {
                    Trace.Error("Stack trace:{0}{1}", Environment.NewLine, ex.StackTrace);
                }
                return (int)ExitCode.CommandLineError;
            }

            // Run the command
            return (int) command.Run(preprocessor);
        }

        private void OutputLogo()
        {
            Console.WriteLine(@"
   ,@@@@@       /@\        @@@@@  
   @@@@@@      @@@@@|     $@@@@@h 
  $@@@@@     ,@@@@@@@    g@@@@@P  
 ]@@@@@M    g@@@@@@@    g@@@@@P   
 $@@@@@    @@@@@@@@@   g@@@@@P    
j@@@@@   g@@@@@@@@@p ,@@@@@@@     
$@@@@@g@@@@@@@@B@@@@@@@@@@@P      
`$@@@@@@@@@@@`  ]@@@@@@@@@`       
  $@@@@@@@P`     ?$@@@@@P         
    `^``           *P*`           ");
        }
    }
}
