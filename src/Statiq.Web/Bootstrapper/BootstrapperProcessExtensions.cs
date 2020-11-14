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

        public static TBootstrapper AddProcess<TBootstrapper>(
            this TBootstrapper bootstrapper,
            ProcessTiming processTiming,
            ProcessLauncher processLauncher)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureProcesses(processes => processes.Add(processTiming, _ => processLauncher));

        public static TBootstrapper AddBackgroundProcess<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string fileName,
            params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddProcess(
                ProcessTiming.BeforeExecution,
                new ProcessLauncher(fileName, arguments)
                {
                    IsBackground = true
                });

        public static TBootstrapper AddProcessBeforeExecution<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string fileName,
            params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddProcess(ProcessTiming.BeforeExecution, new ProcessLauncher(fileName, arguments));

        public static TBootstrapper AddProcessAfterExecution<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string fileName,
            params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddProcess(ProcessTiming.AfterExecution, new ProcessLauncher(fileName, arguments));

        public static TBootstrapper AddProcessBeforeDeployment<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string fileName,
            params string[] arguments)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddProcess(ProcessTiming.BeforeDeployment, new ProcessLauncher(fileName, arguments));
    }
}
