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
using Wyam.Abstractions;

namespace Wyam.Core
{
    public class Engine : IEngine
    {
        private bool _configured = false;

        private readonly Dictionary<string, object> _metadata;

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }

        private readonly Dictionary<string, IReadOnlyList<IDocument>> _completedDocuments 
            = new Dictionary<string, IReadOnlyList<IDocument>>();

        public IReadOnlyDictionary<string, IReadOnlyList<IDocument>> CompletedDocuments
        {
            get { return _completedDocuments; }
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

        private string _rootFolder = Environment.CurrentDirectory;

        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("RootFolder");
                }
                _rootFolder = value;
            }
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
            try
            {
                if(_configured)
                {
                    throw new InvalidOperationException("This engine has already been configured.");
                }
                _configured = true;
                Configurator configurator = new Configurator(this);
                configurator.Configure(configScript);
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }

        public void ConfigureDefaultPipelines()
        {
            // Configure with defaults if not already configured
            if (!_configured)
            {
                Configure();
            }

            try
            {
                Configurator configurator = new Configurator(this);
                configurator.ConfigureDefaultPipelines();
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }

        public void Execute()
        {         
            // Configure with defaults if not already configured
            if(!_configured)
            {
                Configure();
            }

            try
            {
                Trace.Information("Executing {0} pipelines...", _pipelines.Count);
                _completedDocuments.Clear();
                int c = 1;
                foreach(Pipeline pipeline in _pipelines.Pipelines)
                {
                    Trace.Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)...", pipeline.Name, c, _pipelines.Count, pipeline.Count);
                    int indent = Trace.Indent();
                    IReadOnlyList<IDocument> results = pipeline.Execute();
                    _completedDocuments.Add(pipeline.Name, results);
                    Trace.IndentLevel = indent;
                    Trace.Information("Executed pipeline \"{0}\" ({1}/{2}) resulting in {3} output document(s).", pipeline.Name, c++, _pipelines.Count, results.Count);
                }
                Trace.Information("Executed {0} pipelines.", _pipelines.Count);
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }
    }
}
