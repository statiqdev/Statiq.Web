using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Pipelines
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
            return Add(null, false, modules);
        }

        public IPipeline Add(string name, params IModule[] modules)
        {
            return Add(name, false, modules);
        }

        public IPipeline Add(bool processDocumentsOnce, params IModule[] modules)
        {
            return Add(null, processDocumentsOnce, modules);
        }

        public IPipeline Add(string name, bool processDocumentsOnce, params IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Pipeline " + (_pipelines.Count + 1);
            }
            if (_pipelines.ContainsKey(name))
            {
                throw new ArgumentException("Pipelines must have a unique name.");
            }
            Pipeline pipeline = new Pipeline(name, processDocumentsOnce, _engine, modules);
            _pipelines.Add(name, pipeline);
            return pipeline;
        }

        public IEnumerable<Pipeline> Pipelines => _pipelines.Values;

        public int Count
        {
            get { return _pipelines.Count; }
        }

        public bool ContainsKey(string key)
        {
            return _pipelines.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IPipeline value)
        {
            Pipeline pipeline;
            if (_pipelines.TryGetValue(key, out pipeline))
            {
                value = pipeline;
                return true;
            }
            value = null;
            return false;
        }

        public IPipeline this[string key]
        {
            get { return _pipelines[key]; }
        }

        public IEnumerable<string> Keys
        {
            get { return _pipelines.Keys; }
        }

        public IEnumerable<IPipeline> Values
        {
            get { return _pipelines.Values; }
        }

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator()
        {
            return _pipelines
                .Select(x => new KeyValuePair<string, IPipeline>(x.Key, x.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
