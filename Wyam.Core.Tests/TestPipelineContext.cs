using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Tests
{
    public class TestPipelineContext : IPipelineContext
    {
        private readonly Engine _engine;
        private readonly Metadata _metadata;
        private readonly IEnumerable<dynamic> _documents;
        private readonly object _persistedObject;

        public TestPipelineContext(Engine engine, Metadata metadata, IEnumerable<dynamic> documents, object persistedObject)
        {
            _engine = engine;
            _metadata = metadata.Clone();
            _documents = documents;
            _persistedObject = persistedObject;
        }

        public dynamic Metadata
        {
            get { return _metadata; }
        }

        public bool IsReadOnly
        {
            set { _metadata.IsReadOnly = value; }
        }

        public IEnumerable<dynamic> Documents
        {
            get { return _documents; }
        }

        public object ExecutionObject
        {
            get { return _persistedObject; }
        }

        public Trace Trace
        {
            get { return _engine.Trace; }
        }

        public IPipelineContext Clone(object persistedObject)
        {
            return new TestPipelineContext(_engine, _metadata, _documents, persistedObject);
        }
    }
}
