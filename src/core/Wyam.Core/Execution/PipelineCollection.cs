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
        private readonly List<Pipeline> _pipelines = new List<Pipeline>();

        public IPipeline Add(params IModule[] modules) => Add(null, false, modules);

        public IPipeline Add(string name, params IModule[] modules) => Add(name, false, modules);

        public IPipeline Add(bool processDocumentsOnce, params IModule[] modules) => Add(null, processDocumentsOnce, modules);
        
        public IPipeline Add(string name, bool processDocumentsOnce, params IModule[] modules)
        {
            Pipeline pipeline = CreatePipeline(name, processDocumentsOnce, modules);
            _pipelines.Add(pipeline);
            return pipeline;
        }

        public IPipeline InsertBefore(string target, params IModule[] modules) => InsertBefore(target, null, false, modules);

        public IPipeline InsertBefore(string target, string name, params IModule[] modules) => InsertBefore(target, name, false, modules);

        public IPipeline InsertBefore(string target, bool processDocumentsOnce, params IModule[] modules) => 
            InsertBefore(target, null, processDocumentsOnce, modules);

        public IPipeline InsertBefore(string target, string name, bool processDocumentsOnce, params IModule[] modules)
        {
            int index = IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"The pipeline {target} was not found.");
            }

            Pipeline pipeline = CreatePipeline(name, processDocumentsOnce, modules);
            _pipelines.Insert(index, pipeline);
            return pipeline;
        }

        public IPipeline InsertAfter(string target, params IModule[] modules) => InsertAfter(target, null, false, modules);

        public IPipeline InsertAfter(string target, string name, params IModule[] modules) => InsertAfter(target, name, false, modules);

        public IPipeline InsertAfter(string target, bool processDocumentsOnce, params IModule[] modules) =>
            InsertAfter(target, null, processDocumentsOnce, modules);

        public IPipeline InsertAfter(string target, string name, bool processDocumentsOnce, params IModule[] modules)
        {
            int index = IndexOf(target);
            if (index < 0)
            {
                throw new KeyNotFoundException($"The pipeline {target} was not found.");
            }

            Pipeline pipeline = CreatePipeline(name, processDocumentsOnce, modules);
            if (index + 1 < _pipelines.Count)
            {
                _pipelines.Insert(index + 1, pipeline);
            }
            else
            {
                _pipelines.Add(pipeline);
            }
            return pipeline;
        }

        private Pipeline CreatePipeline(string name, bool processDocumentsOnce, params IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Pipeline " + (_pipelines.Count + 1);
            }
            if (ContainsKey(name))
            {
                throw new ArgumentException("Pipelines must have a unique name.");
            }
            return new Pipeline(name, processDocumentsOnce, modules);
        }

        public int IndexOf(string name) => _pipelines.FindIndex(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Pipeline> Pipelines => _pipelines;

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
