using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// TODO Write this
    /// </summary>
    public class SerialCache : ContainerModule
    {
        // Considerations
        // - This module assumes a single input/multiple outputs
        // - Should we cache selectively?
        // - Should we cache into memory for smaller files?
        // TO-DO
        // Consider cacheability based on metadata and source path, not just content.
        // Get a meta key from the user for cache breakage.

        // TODO Move this to key object
        private const string InputHashKey = "SerialCache.InputHash";
        private const string FromCacheKey = "SerialCache.FromCache";
        private const string TemporaryFolderName = @"Wyam\SerialCache";

        private readonly ConcurrentDictionary<string, IList<CachedDocument>> _cachePerHash = new ConcurrentDictionary<string, IList<CachedDocument>>();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="modules">Text</param>
        public SerialCache(params IModule[] modules)
            : base(modules)
        {
        }

        /// <inheritdoc />
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.SelectMany(x => ExecuteAndCache(x, context));
        }

        private IEnumerable<IDocument> ExecuteAndCache(IDocument input, IExecutionContext context)
        {
            // Get the current hash of the input document
            string hashKey = GetHash(input);
            bool fromCache = true;

            // Prep an execution for a lazy run if document is not cached
            Func<IList<CachedDocument>> lazyExecute = () =>
            {
                fromCache = false;
                IEnumerable<IDocument> inputs = ToSingleEnumerable(input);

                List<IDocument> outputs = context.Execute(this, inputs).ToList();
                return outputs.Select(Create).ToList();
            };
            IList<CachedDocument> result = _cachePerHash.GetOrAdd(hashKey, s => lazyExecute());

            // Added possibly helpful metadata.
            Dictionary<string, object> extaMetaData = new Dictionary<string, object>
            {
                {InputHashKey, hashKey},
                {FromCacheKey, fromCache}
            };
            foreach (CachedDocument output in result)
            {
                yield return context.GetDocument(output.FileSource, output.ContentStream, output.MetaData.Concat(extaMetaData));
            }
        }

        private static CachedDocument Create(IDocument document)
        {
            using (Stream stream = document.GetStream())
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), TemporaryFolderName);
                Directory.CreateDirectory(tempFolder);
                string tempFile = Path.Combine(tempFolder, $"{Guid.NewGuid()}.cache");

                // Copy to cache file
                using (FileStream cacheFile = File.OpenWrite(tempFile))
                {
                    stream.CopyTo(cacheFile);
                }
                return new CachedDocument(document.Source, tempFile, document.Metadata);
            }
        }

        private string GetHash(IDocument input)
        {
            using (Stream contentStream = input.GetStream())
            {
                // TODO Conisder other keys to use
                string hash = GetHashKey("key", contentStream);
                return hash;
            }
        }

        private static IEnumerable<T> ToSingleEnumerable<T>(T input)
        {
            yield return input;
        }

        private static string GetHashKey(string key, Stream stream)
        {
            string hash = Crc32.Calculate(stream).ToString("x8");
            return key + hash;
        }

        private class CachedDocument : IDisposable
        {
            // Keep a lock as other Wyam instances may be sharing this directory of cache.
            private readonly FileStream _lock;

            public FilePath FileSource { get; }

            public string ContentPath { get; }

            public Stream ContentStream => File.OpenRead(ContentPath);

            public IMetadata MetaData { get; }

            public CachedDocument(FilePath fileSource, string contentPath, IMetadata metaData, bool lockFile = true)
            {
                FileSource = fileSource;
                ContentPath = contentPath;
                MetaData = metaData;
                if (lockFile)
                {
                    _lock = new FileStream(contentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
            }

            public void Dispose()
            {
                _lock.Dispose();
            }
        }
    }
}