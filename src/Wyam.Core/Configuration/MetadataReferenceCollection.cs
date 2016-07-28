using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Configuration;

namespace Wyam.Core.Configuration
{
    internal class MetadataReferenceCollection : IMetadataReferenceCollection
    {
        private readonly List<object> _metadataReferences = new List<object>();

        public void Add(object metadataReference)
        {
            if (metadataReference != null)
            {
                _metadataReferences.Add(metadataReference);
            }
        }

        public int Count => _metadataReferences.Count;

        public IEnumerator<object> GetEnumerator() => _metadataReferences.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}