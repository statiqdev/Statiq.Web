using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    internal class Document : IDocument
    {
        private readonly Metadata _metadata;
        private readonly string _content = string.Empty;

        internal Document(Metadata metadata)
        {
            _metadata = metadata;
        }

        private Document(Metadata metadata, string content, IEnumerable<KeyValuePair<string, object>> items = null)
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

        public object Get(string key, object defaultValue)
        {
            return _metadata.Get(key, defaultValue);
        }

        public string Content
        {
            get { return _content; }
        }

        public override string ToString()
        {
            return Content;
        }

        public IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(_metadata, content, items);
        }

        public IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Clone(_content, items);
        }
    }
}
