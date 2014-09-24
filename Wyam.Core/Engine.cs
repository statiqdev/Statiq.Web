using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Engine
    {
        private readonly MetadataStack _metadata;

        public dynamic Metadata
        {
            get { return _metadata; }
        }

        private readonly PipelineCollection _pipelines;

        public PipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        public Engine()
        {
            _metadata = new MetadataStack(this);
            _pipelines = new PipelineCollection(this);
        }

        // Configure the engine using a config script or with defaults if null
        public void Configure(string configScript = null)
        {
            // Configure with defaults if no script
            if (string.IsNullOrWhiteSpace(configScript))
            {
                ConfigureDefault();
                return;
            }

            // Add default namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine("using System.IO;");
            scriptBuilder.AppendLine("using Wyam.Core;");
            scriptBuilder.AppendLine("using Wyam.Core.Helpers;");

            // Add namespaces and load assemblies for all found modules
            ConfigureModuleAssemblies(scriptBuilder);

            // Evaluate the config script
            scriptBuilder.AppendLine(string.Format("void Configure(dynamic Metadata, PipelineCollection Pipelines) {{ {0} }}", configScript));
            var configure = CSScript.Evaluator.CreateDelegate(scriptBuilder.ToString());
            configure(Metadata, Pipelines);
        }

        // Configure the engine with default values
        private void ConfigureDefault()
        {
            Metadata.InputFolder = @".\input";
            Metadata.OutputFolder = @".\output";
            
            // TODO: Configure default pipelines
        }

        // TODO: Is there a better way to do this?
        private void ConfigureModuleAssemblies(StringBuilder scriptBuilder)
        {
            HashSet<string> namespaces = new HashSet<string>();
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string assemblyPath in Directory.GetFiles(currentPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    foreach (Type moduleType in assembly.GetTypes().Where(x => typeof(IModule).IsAssignableFrom(x) && !x.IsAbstract && !x.ContainsGenericParameters))
                    {
                        if (namespaces.Add(moduleType.Namespace))
                        {
                            scriptBuilder.AppendLine(string.Format("using {0};", moduleType.Namespace));
                        }
                    }
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
                    Trace.Verbose("Unexpected exception while loading assemblies: {0}.", ex.Message);
                }
            }
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        public void Execute()
        {            
            // Store the final metadata for each pipeline so it can be used from subsiquent pipelines
            List<dynamic> documents = new List<dynamic>();

            // First pass: prepare each pipelines
            List<PipelinePrepareResult> prepareResults = new List<PipelinePrepareResult>();
            int c = 1;
            foreach(Pipeline pipeline in Pipelines)
            {
                Trace.Verbose("Preparing pipeline {0}...", c);
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
