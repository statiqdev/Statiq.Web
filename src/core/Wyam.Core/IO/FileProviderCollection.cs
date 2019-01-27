using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    internal class FileProviderCollection : IFileProviderCollection
    {
        private readonly ConcurrentDictionary<string, IFileProvider> _fileProviders
            = new ConcurrentDictionary<string, IFileProvider>();

        public FileProviderCollection(IFileProvider defaultFileProvider)
        {
            _fileProviders[NormalizedPath.DefaultFileProvider.Scheme] = defaultFileProvider ?? throw new ArgumentNullException(nameof(defaultFileProvider));
        }

        public IReadOnlyDictionary<string, IFileProvider> Providers => _fileProviders.ToImmutableDictionary();

        public void Add(string scheme, IFileProvider provider)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            _fileProviders.AddOrUpdate(scheme, provider, (k, v) => provider);
        }

        public bool Remove(string scheme)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (scheme?.Length == 0)
            {
                throw new ArgumentException("Can not remove the default provider", nameof(scheme));
            }
            IFileProvider removed;
            return _fileProviders.TryRemove(scheme, out removed);
        }

        public IFileProvider Get(string scheme)
        {
            return _fileProviders[scheme];
        }

        public bool TryGet(string scheme, out IFileProvider fileProvider)
        {
            return _fileProviders.TryGetValue(scheme, out fileProvider);
        }
    }
}
