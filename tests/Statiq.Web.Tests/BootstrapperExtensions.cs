using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NUnit.Framework;
using Octokit;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests
{
    public static class BootstrapperExtensions
    {
        public static async Task<ImmutableArray<IDocument>> RunTestAsync(
            this Bootstrapper bootstrapper,
            string outputsPipeline,
            Phase outputsPhase,
            IFileProvider fileProvider)
        {
            ImmutableArray<IDocument> outputs;
            bootstrapper.ConfigureEngine(engine =>
            {
                IPipeline pipeline = engine.Pipelines[outputsPipeline];
                ModuleList modules = null;
                switch (outputsPhase)
                {
                    case Phase.Input:
                        modules = pipeline.InputModules;
                        break;
                    case Phase.Process:
                        modules = pipeline.ProcessModules;
                        break;
                    case Phase.PostProcess:
                        modules = pipeline.PostProcessModules;
                        break;
                    case Phase.Output:
                        modules = pipeline.OutputModules;
                        break;
                }
                modules.Add(new ExecuteConfig(Config.FromContext(ctx =>
                {
                    outputs = ctx.Inputs;
                    return ctx.Inputs;
                })));
                engine.FileSystem.RootPath = "/";
                engine.FileSystem.FileProvider = fileProvider;
            });
            await bootstrapper.RunAsync();
            return outputs;
        }
    }
}
