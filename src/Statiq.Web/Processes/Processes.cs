using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Web
{
    public class Processes : IDisposable
    {
        private readonly List<ProcessLauncherFactory> _processLauncherFactories = new List<ProcessLauncherFactory>();

        private KeyValuePair<ProcessTiming, ProcessLauncher>[] _processLaunchers;

        internal Processes()
        {
        }

        public void Dispose()
        {
            if (_processLaunchers is object)
            {
                foreach (KeyValuePair<ProcessTiming, ProcessLauncher> launcher in _processLaunchers)
                {
                    launcher.Value.Dispose();
                }
            }
        }

        public void Add(ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            Add(null, processTiming, getProcessLauncher);

        public void AddNonPreview(ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            Add(false, processTiming, getProcessLauncher);

        public void AddPreview(ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher) =>
            Add(true, processTiming, getProcessLauncher);

        public void Add(bool? whenPreviewCommand, ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getProcessLauncher)
        {
            if (_processLaunchers is object)
            {
                throw new InvalidOperationException("Cannot add processes once execution has started");
            }
            _processLauncherFactories.Add(new ProcessLauncherFactory
            {
                WhenPreviewCommand = whenPreviewCommand,
                ProcessTiming = processTiming,
                GetProcessLauncher = getProcessLauncher
            });
        }

        public void Clear()
        {
            if (_processLaunchers is object)
            {
                throw new InvalidOperationException("Cannot clear processes once execution has started");
            }
            _processLauncherFactories.Clear();
        }

        internal void CreateProcessLaunchers(bool isPreviewCommand, IExecutionState executionState)
        {
            if (_processLaunchers is null)
            {
                _processLaunchers = _processLauncherFactories
                    .Where(x => !x.WhenPreviewCommand.HasValue || x.WhenPreviewCommand.Value == isPreviewCommand)
                    .Select(launcherFactory => KeyValuePair.Create(launcherFactory.ProcessTiming, launcherFactory.GetProcessLauncher?.Invoke(executionState)))
                    .Where(launcher => launcher.Value is object)
                    .ToArray();
            }
        }

        internal void StartProcesses(ProcessTiming processTiming, IExecutionState executionState)
        {
            foreach (KeyValuePair<ProcessTiming, ProcessLauncher> launcher in _processLaunchers.Where(x => x.Key == processTiming && !x.Value.AreAnyRunning))
            {
                launcher.Value.StartNew(null, null, executionState.Logger, executionState.Services, executionState.CancellationToken);
            }
        }

        internal void WaitForRunningProcesses(ProcessTiming processTiming)
        {
            foreach (KeyValuePair<ProcessTiming, ProcessLauncher> launcher in _processLaunchers.Where(x => x.Key == processTiming))
            {
                launcher.Value.WaitForRunningProcesses();
            }
        }

        private class ProcessLauncherFactory
        {
            public bool? WhenPreviewCommand { get; set; } // null means both preview and non-preview
            public ProcessTiming ProcessTiming { get; set; }
            public Func<IExecutionState, ProcessLauncher> GetProcessLauncher { get; set; }
        }
    }
}
