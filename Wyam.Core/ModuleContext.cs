using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    internal class ModuleContext : IModuleContext
    {
        private readonly Metadata _metadata;
        private readonly string _content = string.Empty;

        internal ModuleContext(Metadata metadata)
        {
            _metadata = metadata;
        }

        private ModuleContext(Metadata metadata, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            _metadata = metadata.Clone(items);
            _content = content ?? string.Empty;
        }

        public IMetadata Metadata
        {
            get { return _metadata; }
        }

        public object this[string key]
        {
            get { return _metadata[key]; }
        }

        public string Content
        {
            get { return _content; }
        }

        // Use the during module prepare to get a fresh context with metadata that can be changed and/or a persisted object
        // The persisted object will be available from the context of the same module during execution
        public IModuleContext Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new ModuleContext(_metadata, content, items);
        }

        public IModuleContext Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Clone(_content, items);
        }
    }
}
