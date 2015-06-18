using System.Collections.Generic;
using Wyam.Abstractions;
using Wyam.Core.NuGet;

namespace Wyam.Core.Configuration
{
    // This gets passed to the scripting engine as a global object and all members can be accessed globally from the script
    public class ConfigGlobals
    {
        private readonly Engine _engine;

        internal ConfigGlobals(Engine engine)
        {
            _engine = engine;
        }

        public IDictionary<string, object> Metadata
        {
            get { return _engine.Metadata; }
        }

        public IPipelineCollection Pipelines
        {
            get { return _engine.Pipelines; }
        }

        public string RootFolder
        {
            get { return _engine.RootFolder; }
        }

        public string InputFolder
        {
            get { return _engine.InputFolder; }
        }

        public string OutputFolder
        {
            get { return _engine.OutputFolder; }
        }
    }
}
