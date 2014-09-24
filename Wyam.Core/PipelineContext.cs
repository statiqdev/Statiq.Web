using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class PipelineContext
    {
        private readonly MetadataStack _metadata;
        private readonly IEnumerable<dynamic> _documents;
        private readonly object _persistedObject;

        internal PipelineContext(MetadataStack metadata, IEnumerable<dynamic> documents)
        {
            _metadata = metadata.Clone();
            _documents = documents;
            Lock();  // Lock the metadata for a new uncloned context - I.e. at the start of the pipeline
        }

        private PipelineContext(MetadataStack metadata, IEnumerable<dynamic> documents, object persistedObject)
        {
            _metadata = metadata.Clone();
            _documents = documents;
            _persistedObject = persistedObject;
        }

        public dynamic Metadata
        {
            get { return _metadata; }
        }

        public IEnumerable<dynamic> Documents
        {
            get { return _documents; }
        }

        public object PersistedObject
        {
            get { return _persistedObject; }
        }

        // Use the during module prepare to get a fresh context with metadata that can be changed and/or a persisted object
        public PipelineContext Clone(object persistedObject)
        {
            return new PipelineContext(_metadata, _documents, persistedObject);
        }

        internal void Lock()
        {
            _metadata.Locked = true;
        }

        internal void Unlock()
        {
            _metadata.Locked = false;
        }
    }
}
