using System;
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
        /// Adds a new process.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process and will be started when not in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddNonPreviewProcess(processTiming, fileName, arguments));

        /// <summary>
        /// Adds a new process that will be started when in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
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
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddConcurrentPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, string fileName, params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddConcurrentPreviewProcess(processTiming, fileName, arguments));

        // Process Launcher

        /// <summary>
        /// Adds a new process.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddProcess(processTiming, getProcessLauncher));

        /// <summary>
        /// Adds a new process that will be started when not in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddNonPreviewProcess(processTiming, getProcessLauncher));

        /// <summary>
        /// Adds a new process that will be started when in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddPreviewProcess(processTiming, getProcessLauncher));

        /// <summary>
        /// Adds a new process.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddProcess(processTiming, waitForExit, getProcessLauncher));

        /// <summary>
        /// Adds a new process that will be started when not in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddNonPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddNonPreviewProcess(processTiming, waitForExit, getProcessLauncher));

        /// <summary>
        /// Adds a new process that will be started when in the preview command.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddPreviewProcess<TBootstrapper>(this TBootstrapper bootstrapper, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.AddPreviewProcess(processTiming, waitForExit, getProcessLauncher));
    }
}
