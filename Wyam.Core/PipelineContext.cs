using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class PipelineContext
    {
        private readonly Engine _engine;
        private readonly Metadata _metadata;
        private readonly IEnumerable<dynamic> _documents;
        private readonly object _persistedObject;

        internal PipelineContext(Engine engine, Metadata metadata, IEnumerable<dynamic> documents)
        {
            _engine = engine;
            _metadata = metadata.Clone();
            _documents = documents;
            IsReadOnly = true;  // Lock the metadata for a new uncloned context - I.e. at the start of the pipeline
        }

        private PipelineContext(Engine engine, Metadata metadata, IEnumerable<dynamic> documents, object persistedObject)
        {
            _engine = engine;
            _metadata = metadata.Clone();
            _documents = documents;
            _persistedObject = persistedObject;
        }

        public Metadata Metadata
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

        public Trace Trace
        {
            get { return _engine.Trace; }
        }

        // Use the during module prepare to get a fresh context with metadata that can be changed and/or a persisted object
        public PipelineContext Clone(object persistedObject)
        {
            return new PipelineContext(_engine, _metadata, _documents, persistedObject);
        }

        internal bool IsReadOnly
        {
            set { _metadata.IsReadOnly = value; }
        }
    }
}
