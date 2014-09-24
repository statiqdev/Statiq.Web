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
        private readonly object _persistedObject;

        internal PipelineContext(MetadataStack metadata)
        {
            _metadata = metadata.Clone();
            Lock();
        }

        private PipelineContext(MetadataStack metadata, object persistedObject)
        {
            _metadata = metadata.Clone();
            _persistedObject = persistedObject;
        }

        public dynamic Metadata
        {
            get { return _metadata; }
        }

        public object PersistedObject
        {
            get { return _persistedObject; }
        }

        public PipelineContext Clone(object persistedObject)
        {
            return new PipelineContext(_metadata, persistedObject);
        }

        internal void Lock()
        {
            _metadata.Lock();
        }
    }
}
