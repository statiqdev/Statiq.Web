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

        public void Add(params IModule[] modules)
        {
            _pipelines.Add(new Pipeline(_engine, modules));
        }

        internal IEnumerable<Pipeline> All
        {
            get { return _pipelines; }
        }
    }
}
