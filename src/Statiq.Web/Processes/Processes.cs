using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Web
{
    public class Processes : IDisposable
    {
        private readonly List<KeyValuePair<ProcessTiming, Func<IExecutionState, ProcessLauncher>>> _launcherFactories =
            new List<KeyValuePair<ProcessTiming, Func<IExecutionState, ProcessLauncher>>>();

        private KeyValuePair<ProcessTiming, ProcessLauncher>[] _launchers;

        internal Processes()
        {
        }

        public void Dispose()
        {
            if (_launchers is object)
            {
                foreach (KeyValuePair<ProcessTiming, ProcessLauncher> launcher in _launchers)
                {
                    launcher.Value.Dispose();
                }
            }
        }

        public void Add(ProcessTiming processTiming, Func<IExecutionState, ProcessLauncher> getLauncher)
        {
            if (_launchers is object)
            {
                throw new InvalidOperationException("Cannot add processes once execution has started");
            }
            _launcherFactories.Add(KeyValuePair.Create(processTiming, getLauncher));
        }

        public void Clear()
        {
            if (_launchers is object)
            {
                throw new InvalidOperationException("Cannot clear processes once execution has started");
            }
            _launcherFactories.Clear();
        }

        internal void CreateProcessLaunchers(IExecutionState executionState)
        {
            if (_launchers is null)
            {
                _launchers = _launcherFactories
                    .Select(launcherFactory => KeyValuePair.Create(launcherFactory.Key, launcherFactory.Value?.Invoke(executionState)))
                    .Where(launcher => launcher.Value is object)
                    .Select(launcher =>
                    {
                        return launcher;
                    })
                    .ToArray();
            }
        }

        internal void StartProcesses(ProcessTiming processTiming, IExecutionState executionState)
        {
            foreach (KeyValuePair<ProcessTiming, ProcessLauncher> launcher in _launchers.Where(x => x.Key == processTiming && !x.Value.AreAnyRunning))
            {
                launcher.Value.StartNew(null, null, executionState.Logger, executionState.Services, executionState.CancellationToken);
            }
        }
    }
}
