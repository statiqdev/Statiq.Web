using System.Collections;
using System.Collections.Generic;
using Wyam.Common;

namespace Wyam.Core.Pipelines
{
    internal class ReadOnlyPipeline : IReadOnlyPipeline
    {
        private readonly IPipeline _pipeline;

        public ReadOnlyPipeline(IPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public IEnumerator<IModule> GetEnumerator()
        {
            return _pipeline.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _pipeline.Count;
        public IModule this[int index] => _pipeline[index];
        public string Name => _pipeline.Name;
        public bool ProcessDocumentsOnce => _pipeline.ProcessDocumentsOnce;
    }
}