using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Modules;
using Wyam.Core.Execution;
using Wyam.Core.Meta;

namespace Wyam.Core.Documents
{
    internal class DocumentFactory : IDocumentFactory
    {
        private readonly MetadataDictionary _settings;

        public DocumentFactory(MetadataDictionary settings)
        {
            _settings = settings;
        }

        public IDocument GetDocument(IExecutionContext context)
        {
            return new Document(_settings, null, null, null, null, true);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null)
        {
            Stream stream = GetTempContentFileStream(context, content);
            if (stream != null)
            {
                return GetDocument(context, sourceDocument, source, stream, items, true);
            }
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, null, content, items, true);
            }
            return new Document((Document)sourceDocument, source, content, items);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            IEnumerable<KeyValuePair<string, object>> items = null)
        {
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, null, null, items, true);
            }
            return new Document((Document)sourceDocument, source, items);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            string content,
            IEnumerable<KeyValuePair<string, object>> items = null)
        {
            Stream stream = GetTempContentFileStream(context, content);
            if (stream != null)
            {
                return GetDocument(context, sourceDocument, stream, items, true);
            }
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, null, null, content, items, true);
            }
            return new Document((Document)sourceDocument, content, items);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            FilePath source,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, source, stream, null, items, disposeStream);
            }
            return new Document((Document)sourceDocument, source, stream, items, disposeStream);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            Stream stream,
            IEnumerable<KeyValuePair<string, object>> items = null,
            bool disposeStream = true)
        {
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, null, stream, null, items, disposeStream);
            }
            return new Document((Document)sourceDocument, stream, items, disposeStream);
        }

        public IDocument GetDocument(
            IExecutionContext context,
            IDocument sourceDocument,
            IEnumerable<KeyValuePair<string, object>> items)
        {
            if (sourceDocument == null || ModuleExtensions.AsNewDocumentModules.Contains(context.Module))
            {
                return new Document(_settings, null, null, null, items, true);
            }
            return new Document((Document)sourceDocument, items);
        }

        private Stream GetTempContentFileStream(IExecutionContext context, string content)
        {
            if (!string.IsNullOrEmpty(content) && _settings.Bool(Keys.UseTempContentFiles))
            {
                IFile tempFile = context.FileSystem.GetTempFile();
                tempFile.WriteAllText(content);
                return new TempFileStream(tempFile);
            }
            return null;
        }
    }
}
