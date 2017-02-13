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

        IEnumerator<KeyValuePair<string, IModule>> IEnumerable<KeyValuePair<string, IModule>>.GetEnumerator() => 
            ((IEnumerable<KeyValuePair<string, IModule>>)_pipeline).GetEnumerator();

        IEnumerator<IModule> IEnumerable<IModule>.GetEnumerator() => _pipeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _pipeline).GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _pipeline.GetEnumerator();

        public int Count => _pipeline.Count;

        public bool ContainsKey(string key) => _pipeline.ContainsKey(key);

        public bool TryGetValue(string key, out IModule value) => _pipeline.TryGetValue(key, out value);

        public IModule this[string key] => _pipeline[key];

        public IModule this[int index] => _pipeline[index];

        public IEnumerable<string> Keys => _pipeline.Keys;

        public IEnumerable<IModule> Values => _pipeline.Values;

        public int IndexOf(string name) => _pipeline.IndexOf(name);
    }
}