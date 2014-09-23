using CSScriptLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Engine
    {
        private readonly VariableStack _vars;

        public dynamic Vars
        {
            get { return _vars; }
        }

        public Engine()
        {
            _vars = new VariableStack(this);
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

            // Evaluate the config script
            var configure = CSScript.Evaluator.CreateDelegate(
                string.Format("void Configure(dynamic vars) {{ {0} }}", configScript));
            configure(Vars);
        }

        // Configure the engine with default values
        private void ConfigureDefault()
        {
            Vars.InputFolder = @".\input";
            Vars.OutputFolder = @".\output";
            
        }

        private readonly Trace _trace = new Trace();

        public Trace Trace
        {
            get { return _trace; }
        }

        // This adds a top-level variable
        public void AddVariable(string key, object value)
        {
            _vars.AddTopLevel(key, value);
        }

        public void Build(IBuildFilter buildFilter = null)
        {
        }
    }
}
