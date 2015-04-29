using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using NuGet;
using Wyam.Core.Configuration;
using Wyam.Core.NuGet;
using Wyam.Extensibility;

namespace Wyam.Core
{
    public class Engine
    {
        private bool _configured = false;

        private readonly Dictionary<string, object> _metadata;

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }

        private readonly List<IModuleContext> _completedContexts = new List<IModuleContext>();

        public IReadOnlyList<IModuleContext> CompletedContexts
        {
            get { return _completedContexts.AsReadOnly(); }
        }

        private readonly PipelineCollection _pipelines;

        public IPipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        private readonly Trace _trace = new Trace();

        public ITrace Trace
        {
            get { return _trace; }
        }

        public Engine()
        {
            _metadata = new Dictionary<string, object>();
            _pipelines = new PipelineCollection(this);
        }

        // This maps Roslyn diagnostic levels to tracing levels
        private static readonly Dictionary<DiagnosticSeverity, TraceEventType> DiagnosticMapping 
            = new Dictionary<DiagnosticSeverity, TraceEventType>()
            {
                { DiagnosticSeverity.Error, TraceEventType.Error },
                { DiagnosticSeverity.Warning, TraceEventType.Warning },
                { DiagnosticSeverity.Info, TraceEventType.Information }
            };

        public void Configure(string configScript = null)
        {
            if(_configured)
            {
                throw new InvalidOperationException("This engine has already been configured.");
            }
            _configured = true;
            Configurator configurator = new Configurator(this);
            configurator.Configure(configScript);
        }

        public void ConfigureDefaultPipelines()
        {
            // Configure with defaults if not already configured
            if (!_configured)
            {
                Configure();
            }

            Configurator configurator = new Configurator(this);
            configurator.ConfigureDefaultPipelines();
        }

        public void Execute()
        {         
            // Configure with defaults if not already configured
            if(!_configured)
            {
                Configure();
            }

            Trace.Verbose("Executing {0} pipelines...", _pipelines.Count);
            _completedContexts.Clear();
            int c = 1;
            foreach(Pipeline pipeline in _pipelines.Pipelines)
            {
                Trace.Verbose("Executing pipeline {0} with {1} child module(s)...", c, pipeline.Count);
                Metadata metadata = new Metadata(this);
                IReadOnlyList<IModuleContext> results = pipeline.Execute(metadata);
                _completedContexts.AddRange(results);
                Trace.Verbose("Executed pipeline {0} resulting in {1} output(s).", c++, results.Count);
            }
            Trace.Verbose("Executed {0} pipelines.", _pipelines.Count);
        }
    }
}
