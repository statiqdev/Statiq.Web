using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Meta;

namespace Wyam.Core.Documents
{
    // Because it's immutable, document metadata can still be accessed after disposal
    // Document source must be unique within the pipeline
    internal class Document : IDocument
    {
        private readonly Metadata _metadata;
        private string _content;
        private Stream _stream;
        private readonly object _streamLock;
        private bool _disposeStream;
        private bool _disposed;

        // Normal constructors

        internal Document(IEnumerable<KeyValuePair<string, object>> initialMetadata, string content = null)
            : this(initialMetadata, string.Empty, null, content, null, true)
        {
        }

        internal Document(IEnumerable<KeyValuePair<string, object>> initialMetadata, string source, Stream stream, string content, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
            : this(Guid.NewGuid().ToString(), new Metadata(initialMetadata), source, stream, null, content, items, disposeStream)
        {
        }
        
        private Document(string id, Metadata metadata, string source, string content, IEnumerable<KeyValuePair<string, object>> items)
            : this(id, metadata, source, null, null, content, items, true)
        {
        }

        private Document(string id, Metadata metadata, string source, Stream stream, object streamLock, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
            : this(id, metadata, source, stream, streamLock, null, items, disposeStream)
        {
        }

        private Document(string id, Metadata metadata, string source, Stream stream, object streamLock, string content, IEnumerable<KeyValuePair<string, object>> items, bool disposeStream)
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

        // Cloning constructors (if source is specified but source document already contains a source, it is ignored and source document source is used)
        internal Document(Document sourceDocument, string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(sourceDocument.Id, sourceDocument._metadata, sourceDocument.Source != string.Empty ? sourceDocument.Source : source, content, items)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(sourceDocument.Id, sourceDocument._metadata, sourceDocument.Source, content, items)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
            : this(sourceDocument.Id, sourceDocument._metadata, sourceDocument.Source != string.Empty ? sourceDocument.Source : source, stream, null, items, disposeStream)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
            : this(sourceDocument.Id, sourceDocument._metadata, sourceDocument.Source, stream, null, items, disposeStream)
        {
            sourceDocument.CheckDisposed();
        }

        internal Document(Document sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
            : this(sourceDocument.Id, sourceDocument._metadata, sourceDocument.Source, sourceDocument._stream,
                sourceDocument._streamLock, sourceDocument._content, items, sourceDocument._disposeStream)
        {
            sourceDocument.CheckDisposed();

            // Don't dispose the stream since the cloned document might be final and get passed to another pipeline, it'll take care of final disposal
            sourceDocument._disposeStream = false;
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

        public FilePath FilePath(string key, FilePath defaultValue = null) => _metadata.FilePath(key, defaultValue);

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null) => _metadata.DirectoryPath(key, defaultValue);

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null) => _metadata.List<T>(key, defaultValue);

        IDocument IMetadata.Document(string key) => _metadata.Document(key);

        public IReadOnlyList<IDocument> Documents(string key) => _metadata.Documents(key);

        public dynamic Dynamic(string key, object defaultValue = null) => _metadata.Dynamic(key, defaultValue);

        public int Count => _metadata.Count;
    }
}
