using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    internal class PipelineCollection : IPipelineCollection
    {
        private readonly Engine _engine;
        private readonly Dictionary<string, Pipeline> _pipelines = new Dictionary<string, Pipeline>();

        public PipelineCollection(Engine engine)
        {
            _engine = engine;
        }

        public IPipeline Add(params IModule[] modules)
        {
            return Add(null, modules);
        }

        public IPipeline Add(string name, params IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Pipeline " + (_pipelines.Count + 1);
            }
            if (_pipelines.ContainsKey(name))
            {
                throw new ArgumentException("Pipelines must have a unique name.");
            }
            Pipeline pipeline = new Pipeline(name, _engine, modules);
            _pipelines.Add(name, pipeline);
            return pipeline;
        }

        public IEnumerable<Pipeline> Pipelines
        {
            get { return _pipelines.Values; }
        }

        public int Count
        {
            get { return _pipelines.Count; }
        }
    }
}
