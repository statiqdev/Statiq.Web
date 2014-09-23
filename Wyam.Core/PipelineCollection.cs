using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class PipelineCollection
    {
        private readonly List<Pipeline> _pipelines = new List<Pipeline>();

        public void Add(Pipeline pipeline)
        {
            _pipelines.Add(pipeline);
        }

        public void Add(params IModule[] modules)
        {
            _pipelines.Add(new Pipeline(modules));
        }
    }
}
