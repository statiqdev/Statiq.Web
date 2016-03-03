using System;
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
        private readonly Dictionary<string, IFileProvider> _fileProviders
            = new Dictionary<string, IFileProvider>();

        public FileProviderCollection(IFileProvider defaultFileProvider)
        {
            if (defaultFileProvider == null)
            {
                throw new ArgumentNullException(nameof(defaultFileProvider));
            }

            _fileProviders[string.Empty] = defaultFileProvider;
        }

        public IReadOnlyDictionary<string, IFileProvider> Providers => _fileProviders.ToImmutableDictionary();

        public bool Add(string name, IFileProvider provider)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            
            if (_fileProviders.ContainsKey(name))
            {
                _fileProviders[name] = provider;
                return true;
            }
            _fileProviders.Add(name, provider);
            return false;
        }

        public bool Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name == string.Empty)
            {
                throw new ArgumentException("Can not remove the default provider", nameof(name));
            }

            return _fileProviders.Remove(name);
        }

        public IFileProvider Get(string name)
        {
            return _fileProviders[name];
        }

        public bool TryGet(string name, out IFileProvider fileProvider)
        {
            return _fileProviders.TryGetValue(name, out fileProvider);
        }
    }
}
