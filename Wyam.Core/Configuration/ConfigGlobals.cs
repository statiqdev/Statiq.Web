using System.Collections.Generic;
using Wyam.Abstractions;
using Wyam.Core.NuGet;

namespace Wyam.Core.Configuration
{
    // This gets passed to the scripting engine as a global object and all members can be accessed globally from the script
    public class ConfigGlobals
    {
        private readonly IDictionary<string, object> _metadata;
        private readonly IPipelineCollection _pipelines;

        internal ConfigGlobals(IDictionary<string, object> metadata, IPipelineCollection pipelines)
        {
            _metadata = metadata;
            _pipelines = pipelines;
        }

        public IDictionary<string, object> Metadata
        {
            get { return _metadata; }
        }

        public IPipelineCollection Pipelines
        {
            get { return _pipelines; }
        }
    }
}
