using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class PipelineBuilder : IPipelineBuilder
    {
        private readonly Pipeline _pipeline;

        public PipelineBuilder(Pipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public IPipelineBuilder AddModule(IModule module)
        {
            _pipeline.Add(module);
            return this;
        }
    }
}
