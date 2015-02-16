using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Tests
{
    public class TestPipelineBuilder : IPipelineBuilder
    {
        public IModule Module { get; set; }

        public IPipelineBuilder AddModule(IModule module)
        {
            Module = module;
            return this;
        }
    }
}
