using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Hosting;

namespace Statiq.Web.Commands
{
    public class ScriptGlobals
    {
        private readonly Action _onExit;

        internal ScriptGlobals(IEngine engine, Action onExit)
        {
            Engine = engine;
            _onExit = onExit;
        }

        public IEngine Engine { get; }

        public IServiceProvider Services => Engine.Services;

        public FilteredDocumentList<IDocument> OutputPages => Engine.OutputPages;

        public IPipelineOutputs Outputs => Engine.Outputs;

        public IReadOnlySettings Settings => Engine.Settings;

        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        public IReadOnlyPipelineCollection Pipelines => Engine.Pipelines;

        public void Exit() => _onExit();

        // TODO: Add a List() command that uses reflection to list all the commands and properties (and then output a message to use it for help)

        // TODO: Add an Execute(params string[] pipelines) command that can run the execution again
    }
}
