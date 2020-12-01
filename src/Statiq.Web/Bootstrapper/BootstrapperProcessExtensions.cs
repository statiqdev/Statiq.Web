using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperProcessExtensions
    {
        /// <summary>
        /// Configures processes to launch.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="action">The configuration action.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper ConfigureProcesses<TBootstrapper>(this TBootstrapper bootstrapper, Action<Processes> action)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<Processes>()));

        // Normal

        /// <summary>
        /// Adds a new process that runs in the background.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when not in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddNonPreviewProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddPreviewProcess(processTiming, fileName, arguments));

        // Background

        /// <summary>
        /// Adds a new process that runs in the background.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddBackgroundProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddBackgroundProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when not in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddBackgroundNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddBackgroundNonPreviewProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddBackgroundPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddBackgroundPreviewProcess(processTiming, fileName, arguments));

        // Concurrent

        /// <summary>
        /// Adds a new process that runs in the background and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddConcurrentProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddConcurrentProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background, will be started when not in the preview command, and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddConcurrentNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddConcurrentNonPreviewProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background, will be started when in the preview command, and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static TBootstrapper AddConcurrentPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddConcurrentPreviewProcess(processTiming, fileName, arguments));
    }
}
