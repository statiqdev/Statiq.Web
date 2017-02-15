using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    internal class PipelineCollection : IPipelineCollection
    {
        private readonly List<IPipeline> _pipelines = new List<IPipeline>();
        private int _nameCounter = 0;

        public IPipeline Add(string name, ModuleList modules)
        {
            Pipeline pipeline = CreatePipeline(name, modules);
            _pipelines.Add(pipeline);
            return pipeline;
        }

        public IPipeline Insert(int index, string name, ModuleList modules)
        {
            Pipeline pipeline = CreatePipeline(name, modules);
            _pipelines.Insert(index, pipeline);
            return pipeline;
        }

        private Pipeline CreatePipeline(string name, ModuleList modules)
        {
            _nameCounter++;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Pipeline " + _nameCounter;
            }
            if (ContainsKey(name))
            {
                throw new ArgumentException("Pipelines must have a unique name.");
            }
            return new Pipeline(name, modules);
        }

        public bool Remove(string name)
        {
            int index = IndexOf(name);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index) => _pipelines.RemoveAt(index);
        
        public int IndexOf(string name) => _pipelines.FindIndex(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<IPipeline> Pipelines => _pipelines;

        public int Count => _pipelines.Count;

        public bool ContainsKey(string key) => 
            _pipelines.Any(x => string.Equals(key, x.Name, StringComparison.OrdinalIgnoreCase));

        public bool TryGetValue(string key, out IPipeline value)
        {
            value = _pipelines.FirstOrDefault(x => string.Equals(key, x.Name, StringComparison.OrdinalIgnoreCase));
            return value != null;
        }

        public IPipeline this[string key]
        {
            get
            {
                IPipeline pipeline;
                if (!TryGetValue(key, out pipeline))
                {
                    throw new KeyNotFoundException($"The pipeline {key} was not found.");
                }
                return pipeline;
            }
        }

        public IEnumerable<string> Keys => _pipelines.Select(x => x.Name);

        public IEnumerable<IPipeline> Values => _pipelines;

        public IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator() => 
            _pipelines.Select(x => new KeyValuePair<string, IPipeline>(x.Name, x)).GetEnumerator();

        IEnumerator<IPipeline> IEnumerable<IPipeline>.GetEnumerator() => _pipelines.GetEnumerator();

        IEnumerator<IPipeline> IPipelineCollection.GetEnumerator() => _pipelines.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _pipelines.GetEnumerator();

        IPipeline IReadOnlyList<IPipeline>.this[int index] => _pipelines[index];
    }
}
