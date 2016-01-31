using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Meta;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Documents
{
    // Because it's immutable, document metadata can still be accessed after disposal
    // Document source must be unique within the pipeline
    internal class Document : IDocument, IDisposable
    {
        private readonly Pipeline _pipeline; 
        private readonly Metadata _metadata;
        private string _content;
        private Stream _stream;
        private readonly object _streamLock;
        private bool _disposeStream;
        private bool _disposed;

        internal Document(IInitialMetadata initialMetadata, Pipeline pipeline)
            : this(initialMetadata, pipeline, string.Empty, null, null, null, true)
        {
        }

        internal Document(IInitialMetadata initialMetadata, Pipeline pipeline, string source, Stream stream, string content, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
            : this(Guid.NewGuid().ToString(), pipeline, new Metadata(initialMetadata), source, stream, null, content, items, disposeStream)
        {
        }
        
        private Document(string id, Pipeline pipeline, Metadata metadata, string source, string content, IEnumerable<KeyValuePair<string, object>> items)
            : this(id, pipeline, metadata, source, null, null, content, items, true)
        {
        }

        private Document(string id, Pipeline pipeline, Metadata metadata, string source, Stream stream, object streamLock, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
            : this(id, pipeline, metadata, source, stream, streamLock, null, items, disposeStream)
        {
        }

        private Document(string id, Pipeline pipeline, Metadata metadata, string source, Stream stream, object streamLock, string content, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Id = id;
            Source = source;
            _metadata = items == null ? metadata : metadata.Clone(items);
            _content = content;

            _pipeline = pipeline;
            _pipeline.AddClonedDocument(this);

            if (stream != null)
            {
                if (!stream.CanRead)
                {
                    throw new ArgumentException("Document stream must support reading.", nameof(stream));
                }

                if (!stream.CanSeek)
                {
                    _stream = new SeekableStream(stream, disposeStream);
                    _disposeStream = true;
                }
                else
                {
                    _stream = stream;
                    _disposeStream = disposeStream;
                }
            }
            _streamLock = stream != null && streamLock != null ? streamLock : new object();
        }

        public string Source { get; }

        public string Id { get; }

        public IMetadata Metadata => _metadata;

        public string Content
        {
            get
            {
                CheckDisposed();

                if (_content == null)
                {
                    Monitor.Enter(_streamLock);
                    try
                    {
                        if (_stream != null)
                        {
                            _stream.Position = 0;
                            using (StreamReader reader = new StreamReader(_stream, Encoding.UTF8, true, 4096, true))
                            {
                                _content = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            _content = string.Empty;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_streamLock);
                    }
                }

                return _content;
            }
        }

        // The stream you get from this call must be disposed as soon as reading is complete
        // Other threads will block until the previous stream is disposed
        public Stream GetStream()
        {
            CheckDisposed();

            Monitor.Enter(_streamLock);

            if (_stream == null)
            {
                if (_content != null)
                {
                    _stream = new MemoryStream(Encoding.UTF8.GetBytes(_content));
                    _disposeStream = true;
                }
                else
                {
                    _stream = Stream.Null;
                }
            }

            _stream.Position = 0;
            return new BlockingStream(_stream, this);
        }

        internal void ReleaseStream() => Monitor.Exit(_streamLock);

        public override string ToString()
        {
            if (_disposed)
            {
                return string.Empty;
            }

            // Return from the buffered string content if available
            if (_content != null)
            {
                return _content.Length < 128 ? _content : _content.Substring(0, 128);
            }

            // Otherwise, use the stream
            Monitor.Enter(_streamLock);
            try
            {
                _stream.Position = 0;
                using (StreamReader reader = new StreamReader(_stream, Encoding.UTF8, true, 4096, true))
                {
                    char[] buffer = new char[128];
                    int count = reader.Read(buffer, 0, 128);
                    return new string(buffer, 0, count);
                }
            }
            finally
            {
                Monitor.Exit(_streamLock);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_disposeStream)
            {
                _stream?.Dispose();
            }
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Document));
            }
        }

        // source is ignored if one is already set (use IExecutionContext.GetNewDocument if you want a whole new document)
        public IDocument Clone(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (Source != string.Empty)
            {
                return Clone(content, items);
            }
            CheckDisposed();
            _pipeline.AddDocumentSource(source);
            return new Document(Id, _pipeline, _metadata, source, content, items);
        }

        public IDocument Clone(string source, string content, IEnumerable<MetadataItem> items) => 
            Clone(source, content, items?.Select(x => x.Pair));

        public IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return new Document(Id, _pipeline, _metadata, Source, content, items);
        }

        public IDocument Clone(string content, IEnumerable<MetadataItem> items) => 
            Clone(content, items?.Select(x => x.Pair));

        // source is ignored if one is already set (use IExecutionContext.GetNewDocument if you want a whole new document)
        public IDocument Clone(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            if (Source != string.Empty)
            {
                return Clone(stream, items, disposeStream);
            }
            CheckDisposed();
            _pipeline.AddDocumentSource(source);
            return new Document(Id, _pipeline, _metadata, source, stream, null, items, disposeStream);
        }

        public IDocument Clone(string source, Stream stream, IEnumerable<MetadataItem> items, bool disposeStream = true) => 
            Clone(source, stream, items?.Select(x => x.Pair), disposeStream);

        public IDocument Clone(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            CheckDisposed();
            return new Document(Id, _pipeline, _metadata, Source, stream, null, items, disposeStream);
        }

        public IDocument Clone(Stream stream, IEnumerable<MetadataItem> items, bool disposeStream = true) => 
            Clone(stream, items?.Select(x => x.Pair), disposeStream);

        public IDocument Clone(IEnumerable<KeyValuePair<string, object>> items)
        {
            CheckDisposed();
            Document cloned = new Document(Id, _pipeline, _metadata, Source, _stream, _streamLock, _content, items, _disposeStream);
            _disposeStream = false;  // Don't dispose the stream since the cloned document might be final and get passed to another pipeline, it'll take care of final disposal
            return cloned;
        }

        public IDocument Clone(IEnumerable<MetadataItem> items) => Clone(items?.Select(x => x.Pair));

        // IMetadata

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _metadata.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool ContainsKey(string key) => _metadata.ContainsKey(key);

        public bool TryGetValue(string key, out object value) => _metadata.TryGetValue(key, out value);

        public object this[string key] => _metadata[key];

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Values;

        public IMetadata<T> MetadataAs<T>() => _metadata.MetadataAs<T>();

        public object Get(string key, object defaultValue) => _metadata.Get(key, defaultValue);

        public T Get<T>(string key) => _metadata.Get<T>(key);

        public T Get<T>(string key, T defaultValue) => _metadata.Get<T>(key, defaultValue);

        public string String(string key, string defaultValue = null) => _metadata.String(key, defaultValue);

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => _metadata.List<T>(key, defaultValue);

        IDocument IMetadata.Document(string key) => _metadata.Document(key);

        public string Link(string key, string defaultValue = null, bool pretty = true) => _metadata.Link(key, defaultValue, pretty);

        public dynamic Dynamic(string key, object defaultValue = null) => _metadata.Dynamic(key, defaultValue);

        public int Count => _metadata.Count;
    }
}
