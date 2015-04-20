using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    // This gets passed to the scripting engine as a global object and all members can be accessed globally from the script
    public class ConfigurationGlobals
    {
        private readonly Engine _engine;

        internal ConfigurationGlobals(Engine engine)
        {
            _engine = engine;
        }

        public IDictionary<string, object> Metadata
        {
            get { return _engine.Metadata; }
        }

        public PipelineCollection Pipelines
        {
            get { return _engine.Pipelines; }
        }
    }
}
