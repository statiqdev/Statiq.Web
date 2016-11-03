using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Testing.Documents;

namespace Wyam.Testing.Execution
{
    public class TestExecutionContext : IExecutionContext
    {
        public IDocument GetDocument() => new TestDocument();

        public IDocument GetDocument(FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new TestDocument(items)
            {
                Source = source,
                Content = content
            };
        }

        public IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new TestDocument(items)
            {
                Content = content
            };
        }

        public IDocument GetDocument(IEnumerable<KeyValuePair<string, object>> items) => new TestDocument(items);

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = source,
                Content = content
            };
        }

        public IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = sourceDocument.Source,
                Content = content
            };
        }

        public IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items)
        {
            return new TestDocument(items == null ? sourceDocument : sourceDocument.Concat(items))
            {
                Id = sourceDocument.Id,
                Source = sourceDocument.Source,
                Content = sourceDocument.Content
            };
        }

        public IDocument GetDocument(IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            throw new NotImplementedException();
        }

        public IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            throw new NotImplementedException();
        }

        public IDocument GetDocument(IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            throw new NotImplementedException();
        }

        public IDocument GetDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }
        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> Keys { get; }
        public IEnumerable<object> Values { get; }
        public IMetadata<T> MetadataAs<T>()
        {
            throw new NotImplementedException();
        }

        public object Get(string key, object defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public object GetRaw(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string key, T defaultValue)
        {
            throw new NotImplementedException();
        }

        public string String(string key, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public FilePath FilePath(string key, FilePath defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public DirectoryPath DirectoryPath(string key, DirectoryPath defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<T> List<T>(string key, IReadOnlyList<T> defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public IDocument Document(string key)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDocument> DocumentList(string key)
        {
            throw new NotImplementedException();
        }

        public dynamic Dynamic(string key, object defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<byte[]> DynamicAssemblies { get; }
        public IReadOnlyCollection<string> Namespaces { get; }
        public IReadOnlyPipeline Pipeline { get; }
        public IModule Module { get; }
        public IExecutionCache ExecutionCache { get; }
        public IReadOnlyFileSystem FileSystem { get; }
        public IReadOnlySettings Settings { get; }
        public IDocumentCollection Documents { get; }
        public IMetadata GlobalMetadata { get; }
        public string ApplicationInput { get; }
        public string GetLink()
        {
            throw new NotImplementedException();
        }

        public string GetLink(IMetadata metadata, bool includeHost = false)
        {
            throw new NotImplementedException();
        }

        public string GetLink(IMetadata metadata, string key, bool includeHost = false)
        {
            throw new NotImplementedException();
        }

        public string GetLink(string path, bool includeHost = false)
        {
            throw new NotImplementedException();
        }

        public string GetLink(string path, string host, DirectoryPath root, bool useHttps, bool hideIndexPages, bool hideExtensions)
        {
            throw new NotImplementedException();
        }

        public string GetLink(NormalizedPath path, bool includeHost = false)
        {
            throw new NotImplementedException();
        }

        public string GetLink(NormalizedPath path, string host, DirectoryPath root, bool useHttps, bool hideIndexPages, bool hideExtensions)
        {
            throw new NotImplementedException();
        }

        public bool TryConvert<T>(object value, out T result)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> metadata)
        {
            throw new NotImplementedException();
        }
    }
}
