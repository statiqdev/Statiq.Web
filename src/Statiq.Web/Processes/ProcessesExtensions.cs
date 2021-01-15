using System;
using Statiq.Common;

namespace Statiq.Web
{
    public static class ProcessesExtensions
    {
        /// <summary>
        /// Adds a new process.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddProcess(this Processes processes, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            processes.AddProcess(null, processTiming, waitForExit, getProcessLauncher);

        /// <summary>
        /// Adds a new process that will be started when not in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddNonPreviewProcess(this Processes processes, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            processes.AddProcess(false, processTiming, waitForExit, getProcessLauncher);

        /// <summary>
        /// Adds a new process that will be started when in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddPreviewProcess(this Processes processes, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            processes.AddProcess(true, processTiming, waitForExit, getProcessLauncher);

        // Normal

        /// <summary>
        /// Adds a new process that runs in the background.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(null, processTiming, false, _ => new ProcessLauncher(fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when not in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddNonPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(false, processTiming, false, _ => new ProcessLauncher(fileName, arguments));

        /// <summary>
        /// Adds a new process that runs in the background and will be started when in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(true, processTiming, false, _ => new ProcessLauncher(fileName, arguments));

        // Background

        /// <summary>
        /// Adds a new process that runs in the background.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddBackgroundProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(null, processTiming, false, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });

        /// <summary>
        /// Adds a new process that runs in the background and will be started when not in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddBackgroundNonPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(false, processTiming, false, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });

        /// <summary>
        /// Adds a new process that runs in the background and will be started when in the preview command.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddBackgroundPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(true, processTiming, false, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });

        // Concurrent

        /// <summary>
        /// Adds a new process that runs in the background and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddConcurrentProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(null, processTiming, true, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });

        /// <summary>
        /// Adds a new process that runs in the background, will be started when not in the preview command, and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddConcurrentNonPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(false, processTiming, true, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });

        /// <summary>
        /// Adds a new process that runs in the background, will be started when in the preview command, and will wait for process exit before starting the next process timing phase.
        /// </summary>
        /// <param name="processes">The processes collection.</param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="fileName">The file name of the process to start.</param>
        /// <param name="arguments">The arguments to pass to the process.</param>
        /// <returns>The process collection.</returns>
        public static Processes AddConcurrentPreviewProcess(this Processes processes, ProcessTiming processTiming, string fileName, params string[] arguments) =>
            processes.AddProcess(true, processTiming, true, _ => new ProcessLauncher(fileName, arguments)
            {
                IsBackground = true
            });
    }
}
