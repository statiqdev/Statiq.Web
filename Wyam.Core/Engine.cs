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
using NuGet;
using Wyam.Core.Configuration;
using Wyam.Core.NuGet;
using Wyam.Abstractions;
using Wyam.Core.Caching;
using Wyam.Core.Helpers;
using Wyam.Core.Tracing;

namespace Wyam.Core
{
    public class Engine : IDisposable
    {
        private Configurator _configurator = null;
        private bool _disposed;

        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();

        // This is used as the initial set of metadata for each run
        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }
        
        private readonly DocumentCollection _documents = new DocumentCollection();

        public IDocumentCollection Documents
        {
            get { return _documents; }
        }

        internal DocumentCollection DocumentCollection
        {
            get {  return _documents; }
        }

        private readonly PipelineCollection _pipelines;

        public IPipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        private readonly Tracing.Trace _trace = new Tracing.Trace();

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

        private readonly ExecutionCacheManager _executionCacheManager = new ExecutionCacheManager();

        internal ExecutionCacheManager ExecutionCacheManager
        {
            get { return _executionCacheManager; }
        }

        public bool NoCache
        {
            get { return _executionCacheManager.NoCache; }
            set { _executionCacheManager.NoCache = value; }
        }
        
        private string _rootFolder = Environment.CurrentDirectory;
        private string _inputFolder = "Input";
        private string _outputFolder = "Output";

        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("RootFolder");
                }
                _rootFolder = Path.GetFullPath(PathHelper.NormalizePath(value));
            }
        }

        public string InputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return Path.GetFullPath(Path.Combine(RootFolder, PathHelper.NormalizePath(_inputFolder)));
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("InputFolder");
                }
                _inputFolder = PathHelper.NormalizePath(value);;
            }
        }

        public string OutputFolder
        {
            get
            {
                // Calculate this each time in case the root folder changes after setting it
                return Path.GetFullPath(Path.Combine(RootFolder, PathHelper.NormalizePath(_outputFolder)));
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("OutputFolder");
                }
                _outputFolder = PathHelper.NormalizePath(value);
            }
        }

        public Engine()
        {
            _pipelines = new PipelineCollection(this);
        }

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
            
            try
            {
                using (Trace.WithIndent().Information("Executing {0} pipelines", _pipelines.Count))
                {
                    _documents.Clear();
                    _executionCacheManager.ResetEntryHits();
                    int c = 1;
                    foreach(Pipeline pipeline in _pipelines.Pipelines)
                    {
                        using (Trace.WithIndent().Information("Executing pipeline \"{0}\" ({1}/{2}) with {3} child module(s)", pipeline.Name, c, _pipelines.Count, pipeline.Count))
                        {
                            pipeline.Execute();
                        }
                        Trace.Information("Executed pipeline \"{0}\" ({1}/{2}) resulting in {3} output document(s)", 
                            pipeline.Name, c++, _pipelines.Count, _documents.FromPipeline(pipeline.Name).Count());
                    }
                    _executionCacheManager.ClearUnhitEntries();
                }
            }
            catch (Exception ex)
            {
                Trace.Verbose("Exception while executing pipelines: {0}", ex);
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
