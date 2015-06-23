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
    public class Engine : IDisposable
    {
        private Configurator _configurator = null;
        private bool _disposed;

        private readonly Dictionary<string, object> _metadata;

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }
        
        private readonly Dictionary<string, IReadOnlyList<IDocument>> _documents 
            = new Dictionary<string, IReadOnlyList<IDocument>>();

        public IReadOnlyDictionary<string, IReadOnlyList<IDocument>> Documents
        {
            get { return _documents; }
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
        
        public byte[] RawConfigAssembly
        {
            get { return _configurator == null ? null : _configurator.RawConfigAssembly; }
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _configurator == null ? null : _configurator.Assemblies; }
        }
        
        private string _rootFolder = Environment.CurrentDirectory;
        private string _inputFolder = @".\Input";
        private string _outputFolder = @".\Output";

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

        public string InputFolder
        {
            get { return Path.Combine(RootFolder, _inputFolder); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("InputFolder");
                }
                _inputFolder = value;
            }
        }

        public string OutputFolder
        {
            get { return Path.Combine(RootFolder, _outputFolder); }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("OutputFolder");
                }
                _outputFolder = value;
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

        public void Configure(string configScript = null, bool updatePackages = false)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Engine");
            }

            try
            {
                if(_configurator != null)
                {
                    throw new InvalidOperationException("This engine has already been configured.");
                }
                _configurator = new Configurator(this);
                _configurator.Configure(configScript, updatePackages);
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }

        public void Execute()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Engine");
            }

            // Configure with defaults if not already configured
            if(_configurator == null)
            {
                Configure();
            }

            // Create the input and output folders if they don't already exist
            if (!Directory.Exists(InputFolder))
            {
                Directory.CreateDirectory(InputFolder);
            }
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            int outerIndent = Trace.IndentLevel;
            try
            {
                Trace.Information("Executing {0} pipelines...", _pipelines.Count);
                outerIndent = Trace.Indent();
                _documents.Clear();
                int c = 1;
                foreach(Pipeline pipeline in _pipelines.Pipelines)
                {
                    Trace.Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)...", pipeline.Name, c, _pipelines.Count, pipeline.Count);
                    int indent = Trace.Indent();
                    string pipelineName = pipeline.Name;
                    int documentCount = 0;
                    pipeline.Execute(x =>
                    {
                        _documents[pipelineName] = x;
                        documentCount = x.Count;
                    });
                    Trace.IndentLevel = indent;
                    Trace.Information("Executed pipeline \"{0}\" ({1}/{2}) resulting in {3} output document(s).", pipeline.Name, c++, _pipelines.Count, documentCount);
                }
                Trace.IndentLevel = outerIndent;
                Trace.Information("Executed {0} pipelines.", _pipelines.Count);
            }
            catch (Exception ex)
            {
                Trace.IndentLevel = outerIndent;
                Trace.Verbose("Exception: {0}", ex);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Engine");
            }
            _disposed = true;
            _trace.Dispose();
            if (_configurator != null)
            {
                _configurator.Dispose();
            }
        }
    }
}
