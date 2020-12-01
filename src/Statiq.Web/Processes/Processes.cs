using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Web
{
    /// <summary>
    /// Keeps track of process and when they should be started.
    /// </summary>
    public class Processes : IDisposable
    {
        private readonly List<ProcessLauncherFactory> _processLauncherFactories = new List<ProcessLauncherFactory>();

        private KeyValuePair<ProcessLauncherFactory, ProcessLauncher>[] _processLaunchers;

        internal Processes()
        {
        }

        public void Dispose()
        {
            if (_processLaunchers is object)
            {
                foreach (KeyValuePair<ProcessLauncherFactory, ProcessLauncher> launcher in _processLaunchers)
                {
                    launcher.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Adds a new process.
        /// </summary>
        /// <param name="previewCommand">
        /// <c>true</c> to start the process for the preview command,
        /// <c>false</c> to start the process for non-preview commands,
        /// <c>null</c> to always start the process.
        /// </param>
        /// <param name="processTiming">When to start the process.</param>
        /// <param name="waitForExit">
        /// <c>true</c> to wait for this process to exit before the next process timing phase, <c>false</c> to allow it to continue running in the background.
        /// This flag is only needed when <see cref="ProcessLauncher.IsBackground"/> is <c>true</c>, otherwise the process will block until it exits.
        /// </param>
        /// <param name="getProcessLauncher">A factory that returns a process launcher.</param>
        /// <returns>The process collection.</returns>
        public Processes AddProcess(bool? previewCommand, ProcessTiming processTiming, bool waitForExit, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
        {
            if (_processLaunchers is object)
            {
                throw new InvalidOperationException("Cannot add processes once execution has started");
            }
            _processLauncherFactories.Add(new ProcessLauncherFactory
            {
                PreviewCommand = previewCommand,
                ProcessTiming = processTiming,
                WaitForExit = waitForExit,
                GetProcessLauncher = getProcessLauncher
            });
            return this;
        }

        public void Clear()
        {
            if (_processLaunchers is object)
            {
                throw new InvalidOperationException("Cannot clear processes once execution has started");
            }
            _processLauncherFactories.Clear();
        }

        internal void CreateProcessLaunchers(bool previewCommand, IExecutionState executionState)
        {
            if (_processLaunchers is null)
            {
                _processLaunchers = _processLauncherFactories
                    .Where(x => !x.PreviewCommand.HasValue || x.PreviewCommand.Value == previewCommand)
                    .Select(launcherFactory => KeyValuePair.Create(launcherFactory, launcherFactory.GetProcessLauncher?.Invoke(executionState)))
                    .Where(launcher => launcher.Value is object)
                    .ToArray();
            }
        }

        internal void StartProcesses(ProcessTiming processTiming, IExecutionState executionState)
        {
            foreach (KeyValuePair<ProcessLauncherFactory, ProcessLauncher> launcher in _processLaunchers
                .Where(x => x.Key.ProcessTiming == processTiming && !x.Value.AreAnyRunning))
            {
                launcher.Value.StartNew(null, null, executionState.Logger, executionState.Services, executionState.CancellationToken);
            }
        }

        /// <summary>
        /// Waits on all processes with the <see cref="ProcessLauncherFactory.WaitForExit"/> flag started during the specified timing.
        /// </summary>
        internal void WaitForRunningProcesses(ProcessTiming processTiming)
        {
            foreach (KeyValuePair<ProcessLauncherFactory, ProcessLauncher> launcher in _processLaunchers
                .Where(x => x.Key.ProcessTiming == processTiming && x.Key.WaitForExit))
            {
                launcher.Value.WaitForRunningProcesses();
            }
        }

        private class ProcessLauncherFactory
        {
            public bool? PreviewCommand { get; set; } // null means both preview and non-preview
            public ProcessTiming ProcessTiming { get; set; }
            public bool WaitForExit { get; set; }
            public Func<IExecutionState, ProcessLauncher> GetProcessLauncher { get; set; }
        }
    }
}
