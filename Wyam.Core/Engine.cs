using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using NuGet;
using Wyam.Core.Configuration;
using Wyam.Core.Extensibility;

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

        private readonly PipelineCollection _pipelines;

        public IPipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        // Store the final metadata for each pipeline so it can be used from subsequent pipelines
        private readonly List<IMetadata> _allMetadata = new List<IMetadata>();

        // This is only populated after running the engine
        public IReadOnlyList<IMetadata> AllMetadata
        {
            get { return _allMetadata; }
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

        public void Run()
        {         
            // Configure with defaults if not already configured
            if(!_configured)
            {
                Configure();
            }

            // First pass: prepare each pipelines
            _allMetadata.Clear();
            List<PipelinePrepareResult> prepareResults = new List<PipelinePrepareResult>();
            int c = 1;
            foreach(Pipeline pipeline in _pipelines.Pipelines)
            {
                Trace.Verbose("Preparing pipeline {0} with {1} modules...", c, pipeline.Count);
                Metadata metadata = new Metadata(this);
                PrepareTree prepareTree = pipeline.Prepare(metadata, _allMetadata);
                prepareResults.Add(new PipelinePrepareResult(pipeline, prepareTree));
                IEnumerable<IMetadata> pipelineMetadata = prepareTree.Leaves.Select(x => x.Context.Metadata).ToList();
                _allMetadata.AddRange(pipelineMetadata);
                Trace.Verbose("Prepared pipeline {0} resulting in {1} documents.", c++, pipelineMetadata.Count());
            }

            // Second pass: execute each pipeline
            c = 1;
            foreach(PipelinePrepareResult prepareResult in prepareResults)
            {
                Trace.Verbose("Executing pipeline {0}...", c);
                prepareResult.Pipeline.Execute(prepareResult.PrepareTree.RootBranch);
                Trace.Verbose("Executed pipeline {0}.", c++);
            }
        }

        private class PipelinePrepareResult
        {
            public Pipeline Pipeline { get; private set; }
            public PrepareTree PrepareTree { get; private set; }

            public PipelinePrepareResult(Pipeline pipeline, PrepareTree prepareTree)
            {
                Pipeline = pipeline;
                PrepareTree = prepareTree;
            }
        }
    }
}
