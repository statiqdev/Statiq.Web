using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class PipelineCollection
    {
        private readonly Engine _engine;
        private readonly List<Pipeline> _pipelines = new List<Pipeline>();

        internal PipelineCollection(Engine engine)
        {
            _engine = engine;
        }

        public IPipelineBuilder Create()
        {
            Pipeline pipeline = new Pipeline(_engine);
            _pipelines.Add(pipeline);
            return new PipelineBuilder(pipeline);
        }

        internal IEnumerable<Pipeline> All
        {
            get { return _pipelines; }
        }
    }
}
