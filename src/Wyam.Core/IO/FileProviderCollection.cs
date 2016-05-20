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
            if (defaultFileProvider == null)
            {
                throw new ArgumentNullException(nameof(defaultFileProvider));
            }

            _fileProviders[NormalizedPath.DefaultProvider.Scheme] = defaultFileProvider;
        }

        public IReadOnlyDictionary<string, IFileProvider> Providers => _fileProviders.ToImmutableDictionary();

        /// <summary>
        /// Adds the specified provider.
        /// </summary>
        /// <param name="scheme">The scheme the provider supports.</param>
        /// <param name="provider">The provider.</param>
        /// <returns>
        /// <c>true</c> if the scheme was new and the provider was added, 
        /// <c>false</c> if a provider already existed with the specified scheme and it was replaced.
        /// </returns>
        public bool Add(string scheme, IFileProvider provider)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            bool updated = false;
            _fileProviders.AddOrUpdate(scheme, provider, (k, v) =>
            {
                updated = true;
                return provider;
            });
            return !updated;
        }

        public bool Remove(string scheme)
        {
            if (scheme == null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (scheme == string.Empty)
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
