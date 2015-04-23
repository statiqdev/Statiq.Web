using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class ModuleContext : IModuleContext
    {
        private readonly Engine _engine;
        private readonly Metadata _metadata;
        private readonly IEnumerable<IMetadata> _allMetadata;
        private readonly object _persistedObject;

        internal ModuleContext(Engine engine, Metadata metadata, IEnumerable<IMetadata> allMetadata)
        {
            _engine = engine;
            _metadata = metadata;
            _allMetadata = allMetadata;
        }

        private ModuleContext(Engine engine, Metadata metadata, IEnumerable<IMetadata> allMetadata, object persistedObject, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            _engine = engine;
            _metadata = metadata.Clone(items);
            _allMetadata = allMetadata;
            _persistedObject = persistedObject;
        }

        public IMetadata Metadata
        {
            get { return _metadata; }
        }

        public IEnumerable<IMetadata> AllMetadata
        {
            get { return _allMetadata; }
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
        // The persisted object will be available from the context of the same module during execution
        public IModuleContext Clone(object persistedObject, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new ModuleContext(_engine, _metadata, _allMetadata, persistedObject, items);
        }

        public IModuleContext Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Clone(null, items);
        }
    }
}
