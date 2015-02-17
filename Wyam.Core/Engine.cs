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

namespace Wyam.Core
{
    public class Engine
    {
        private bool _configured = false;

        private readonly Metadata _metadata;

        // Cast this to Metadata for more direct control - I.e., for .Get() and .Set()
        public dynamic Metadata
        {
            get { return _metadata; }
        }

        private readonly PipelineCollection _pipelines;

        public PipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        public Engine()
        {
            _metadata = new Metadata(this);
            _pipelines = new PipelineCollection(this);
        }

        private static readonly Dictionary<DiagnosticSeverity, TraceEventType> DiagnosticMapping 
            = new Dictionary<DiagnosticSeverity, TraceEventType>()
            {
                { DiagnosticSeverity.Error, TraceEventType.Error },
                { DiagnosticSeverity.Warning, TraceEventType.Warning },
                { DiagnosticSeverity.Info, TraceEventType.Information }
            };

        // Configure the engine using a config script or with defaults if null
        public void Configure(string configScript = null)
        {
            if(_configured)
            {
                throw new InvalidOperationException("This engine has already been configured.");
            }
            _configured = true;

            try
            {
                // Configure with defaults if no script
                if (string.IsNullOrWhiteSpace(configScript))
                {
                    ConfigureDefault();
                    return;
                }

                // Create the script options
                // TODO: Add a way to specify additional namespaces and/or assemblies (probably as arguments, exposed as switches in the console version)
                ScriptOptions scriptOptions = new ScriptOptions()
                    .AddNamespaces(
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.IO", 
                        "Wyam.Core", 
                        "Wyam.Core.Modules", 
                        "Wyam.Core.Helpers")
                    .AddReferences(
                        Assembly.GetAssembly(typeof(object)),  // System
                        Assembly.GetAssembly(typeof(List<>)),  // System.Collections.Generic 
                        Assembly.GetAssembly(typeof(ImmutableArrayExtensions)),  // System.Linq
                        Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Core (needed for dynamic)
                        Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp (needed for dynamic)
                        Assembly.GetAssembly(typeof(Path)), // System.IO
                        Assembly.GetAssembly(typeof(Engine)));  // Wyam.Core
                scriptOptions = AddModulesToScriptOptions(scriptOptions);

                // Evaluate the script
                CSharpScript.Eval(configScript, scriptOptions, new ConfigurationGlobals(this));
            }
            catch(CompilationErrorException compilationError)
            {
                Trace.Error("Error compiling configuration: {0}", compilationError.ToString());
                throw;
            }
            catch(Exception ex)
            {
                Trace.Error("Unexpected error during configuration: {0}", ex.ToString());
                throw;
            }
        }

        // Gets all modules in the current path and adds their namespaces and references to the options
        private ScriptOptions AddModulesToScriptOptions(ScriptOptions scriptOptions)
        {
            List<Assembly> assemblies = new List<Assembly>();
            HashSet<string> namespaces = new HashSet<string>();
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string assemblyPath in Directory.GetFiles(currentPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    foreach (Type moduleType in assembly.GetTypes().Where(x => typeof(IModule).IsAssignableFrom(x) && !x.IsAbstract && !x.ContainsGenericParameters))
                    {
                        namespaces.Add(moduleType.Namespace);
                    }
                    assemblies.Add(assembly);
                }
                catch (FileLoadException)
                {
                    // The Assembly has already been loaded
                }
                catch (BadImageFormatException)
                {
                    // If a BadImageFormatException exception is thrown, the file is not an assembly
                }
                catch (Exception ex)
                {
                    // Some other reason the assembly couldn't be loaded or we couldn't reflect
                    Trace.Verbose("Unexpected exception while loading assembly at {0}: {1}.", assemblyPath, ex.Message);
                }
            }
            return scriptOptions
                .AddNamespaces(namespaces)
                .AddReferences(assemblies);
        }

        // Configure the engine with default values
        private void ConfigureDefault()
        {
            Metadata.InputPath = @".\input";
            Metadata.OutputPath = @".\output";
            
            // TODO: Configure default pipelines
        }

        public void Run()
        {         
            // Configure with defaults if not already configured
            if(!_configured)
            {
                Configure();
            }

            // Store the final metadata for each pipeline so it can be used from subsequent pipelines
            List<dynamic> documents = new List<dynamic>();

            // First pass: prepare each pipelines
            List<PipelinePrepareResult> prepareResults = new List<PipelinePrepareResult>();
            int c = 1;
            foreach(Pipeline pipeline in Pipelines.All)
            {
                Trace.Verbose("Preparing pipeline {0} with {1} modules...", c, pipeline.Count);
                PrepareTree prepareTree = pipeline.Prepare(_metadata, documents);
                prepareResults.Add(new PipelinePrepareResult(pipeline, prepareTree));
                IEnumerable<dynamic> pipelineDocuments = prepareTree.Leaves.Select(x => x.Input.Metadata);
                documents.AddRange(pipelineDocuments);
                Trace.Verbose("Prepared pipeline {0} resulting in {1} documents.", c++, pipelineDocuments.Count());
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
