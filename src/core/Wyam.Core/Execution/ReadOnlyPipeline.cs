using System.Collections;
using System.Collections.Generic;
using Wyam.Common;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Execution
{
    internal class ReadOnlyPipeline : IReadOnlyPipeline
    {
        private readonly IPipeline _pipeline;

        public ReadOnlyPipeline(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public string Name => _pipeline.Name;

        public bool ProcessDocumentsOnce => _pipeline.ProcessDocumentsOnce;
        
        IEnumerator<IModule> IEnumerable<IModule>.GetEnumerator() => _pipeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _pipeline.GetEnumerator();

        public int Count => _pipeline.Count;

        public bool Contains(string name) => _pipeline.Contains(name);

        public bool TryGetValue(string name, out IModule value) => _pipeline.TryGetValue(name, out value);

        public IModule this[string name] => _pipeline[name];

        public IModule this[int index] => _pipeline[index];

        public int IndexOf(string name) => _pipeline.IndexOf(name);

        public IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs() => _pipeline.AsKeyValuePairs();
    }
}