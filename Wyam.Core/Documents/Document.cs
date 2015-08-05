using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wyam.Abstractions;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Documents
{
    internal class Document : IDocument, IDisposable
    {
        private readonly Pipeline _pipeline; 
        private readonly Metadata _metadata;
        private string _content;
        private Stream _contentStream;
        private bool _disposeContentStream;
        private bool _disposed;

        internal Document(Metadata metadata, Pipeline pipeline)
            : this("Initial Document", metadata, (string)null, pipeline)
        {
            _metadata = metadata;
        }
        
        private Document(string source, Metadata metadata, string content, Pipeline pipeline, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(nameof(source));
            }

            Source = source;
            _metadata = metadata.Clone(items);
            _content = content;

            _pipeline = pipeline;
            _pipeline.AddClonedDocument(this);
        }

        private Document(string source, Metadata metadata, Stream contentStream, Pipeline pipeline, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(source, metadata, (string)null, pipeline, items)
        {
            if (contentStream != null)
            {
                if (!contentStream.CanRead)
                {
                    throw new ArgumentException("Document stream must support reading.", nameof(contentStream));
                }

                if (!contentStream.CanSeek)
                {
                    _contentStream = new SeekableStream(contentStream);
                    _disposeContentStream = true;
                }
                else
                {
                    _contentStream = contentStream;
                }
            }
        }

        public string Source { get; }

        public IMetadata Metadata => _metadata;

        public string Content
        {
            get
            {
                CheckDisposed();

                if (_content == null)
                {
                    if (_contentStream != null)
                    {
                        using (StreamReader reader = new StreamReader(_contentStream, true))
                        {
                            _content = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        _content = string.Empty;
                    }
                }

                return _content;
            }
        }

        public Stream ContentStream
        {
            get
            {
                CheckDisposed();

                if (_contentStream == null)
                {
                    if (_content != null)
                    {
                        _contentStream = new MemoryStream(Encoding.UTF8.GetBytes(_content));
                        _disposeContentStream = true;
                    }
                    else
                    {
                        _contentStream = Stream.Null;
                    }
                }

                return _contentStream;
            }
        }

        public void ResetContentStream()
        {
            if (_contentStream != null)
            {
                _contentStream.Position = 0;
            }
        }

        public override string ToString()
        {
            if (_disposed)
            {
                return string.Empty;
            }

            if (_content != null)
            {
                return _content.Length < 128 ? _content : _content.Substring(0, 128);
            }
            using (StreamReader reader = new StreamReader(_contentStream, true))
            {
                char[] buffer = new char[128];
                int count = reader.Read(buffer, 0, 128);
                return new string(buffer, 0, count);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_disposeContentStream)
            {
                _contentStream.Dispose();
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

        public IDocument Clone(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return new Document(source, _metadata, content, _pipeline, items);
        }

        public IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return new Document(Source, _metadata, content, _pipeline, items);
        }

        public IDocument Clone(string source, Stream contentStream, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return new Document(source, _metadata, contentStream, _pipeline, items);
        }

        public IDocument Clone(Stream contentStream, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return new Document(Source, _metadata, contentStream, _pipeline, items);
        }

        public IDocument Clone(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            CheckDisposed();
            return Clone(Content, items);
        }

        // IMetadata

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return _metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _metadata.TryGetValue(key, out value);
        }

        public object this[string key] => _metadata[key];

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Values;

        public IMetadata<T> MetadataAs<T>()
        {
            return _metadata.MetadataAs<T>();
        }

        public object Get(string key, object defaultValue)
        {
            return _metadata.Get(key, defaultValue);
        }

        public T Get<T>(string key)
        {
            return _metadata.Get<T>(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return _metadata.Get<T>(key, defaultValue);
        }

        public string String(string key, string defaultValue = null)
        {
            return _metadata.String(key, defaultValue);
        }

        public string Link(string key, string defaultValue = null)
        {
            return _metadata.Link(key, defaultValue);
        }

        public int Count => _metadata.Count;
    }
}
