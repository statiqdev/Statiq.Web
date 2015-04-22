using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class PipelineCollection : IPipelineCollection
    {
        private readonly Engine _engine;
        private readonly List<Pipeline> _pipelines = new List<Pipeline>();

        public PipelineCollection(Engine engine)
        {
            _engine = engine;
        }

        public IPipeline AddPipeline()
        {
            Pipeline pipeline = new Pipeline(_engine);
            _pipelines.Add(pipeline);
            return pipeline;
        }

        public IEnumerable<Pipeline> Pipelines
        {
            get { return _pipelines; }
        }
    }
}
