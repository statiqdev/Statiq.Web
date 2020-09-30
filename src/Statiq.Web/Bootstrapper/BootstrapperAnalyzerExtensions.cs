using System;
using System.Threading.Tasks;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.App;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperAnalyzerExtensions
    {
        public static TBootstrapper AnalyzeMarkdown<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            Func<MarkdownDocument, IDocument, IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, new DelegateMarkdownAnalyzer(logLevel, analyzeFunc)));

        public static TBootstrapper AnalyzeMarkdown<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            Action<MarkdownDocument, IDocument, IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper
        {
            analyzeAction.ThrowIfNull(nameof(analyzeAction));
            return bootstrapper.AnalyzeMarkdown(
                name,
                logLevel,
                (m, d, c) =>
                {
                    analyzeAction(m, d, c);
                    return Task.CompletedTask;
                });
        }
    }
}
