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
        private readonly MetadataStack _meta;

        public dynamic Meta
        {
            get { return _meta; }
        }

        private readonly PipelineCollection _pipelines = new PipelineCollection();

        public PipelineCollection Pipelines
        {
            get { return _pipelines; }
        }

        public Engine()
        {
            _meta = new MetadataStack(this);
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
                string.Format("void Configure(dynamic Meta, PipelineCollection Pipelines) {{ {0} }}", configScript));
            configure(Meta);
        }

        // Configure the engine with default values
        private void ConfigureDefault()
        {
            Meta.InputFolder = @".\input";
            Meta.OutputFolder = @".\output";
            
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        public void Build(IBuildFilter buildFilter = null)
        {
        }
    }
}
