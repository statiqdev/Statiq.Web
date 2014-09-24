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

        private readonly PipelineCollection _pipelines = new PipelineCollection();

        public PipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        public Engine()
        {
            _metadata = new MetadataStack(this);
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

            // Add assemblies
            CSScript.Evaluator.Run(string.Format("using System.IO;"));
            CSScript.Evaluator.Run(string.Format("using Wyam.Core;"));
            CSScript.Evaluator.Run(string.Format("using Wyam.Core.Modules;"));
            // TODO: Locate modules and add assemblies and using statements for all located modules

            // Evaluate the config script
            var configure = CSScript.Evaluator.CreateDelegate(
                string.Format("void Configure(dynamic Metadata, PipelineCollection Pipelines) {{ {0} }}", configScript));
            configure(Metadata, Pipelines);
        }

        // Configure the engine with default values
        private void ConfigureDefault()
        {
            Metadata.InputFolder = @".\input";
            Metadata.OutputFolder = @".\output";
            
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        public void Execute(IExecuteFilter executeFilter = null)
        {
            // TODO: Use the filter
            
            // Prepare all of the pipelines
            foreach(Pipeline pipeline in Pipelines)
            {

            }
        }
    }
}
