using System;
using System.Linq;

namespace Wyam.Razor
{
    /// <summary>
    /// Used as a key for the Razor compiler cache to persist page compilations from one generation to the next.
    /// A composite of layout location, view start location, and file hash. Note that file path is not included
    /// so that documents with the same content (or not content) and with the same layout and view start can
    /// use the same cached compilation.
    /// </summary>
    internal class CompilerCacheKey : IEquatable<CompilerCacheKey>
    {
        private readonly RenderRequest _request;
        private readonly byte[] _fileHash;
        private readonly int _hashCode;

        public CompilerCacheKey(RenderRequest request, byte[] fileHash)
        {
            _request = request;
            _fileHash = fileHash;

            // Precalculate hash code since we know we'll need it
            _hashCode = 17;
            _hashCode = (_hashCode * 31) + (_request.LayoutLocation?.GetHashCode() ?? 0);
            _hashCode = (_hashCode * 31) + (_request.ViewStartLocation?.GetHashCode() ?? 0);
            foreach (byte b in _fileHash)
            {
                _hashCode = (_hashCode * 31) ^ b;
            }
        }

        public override int GetHashCode() => _hashCode;

        public override bool Equals(object obj) => Equals(obj as CompilerCacheKey);

        public bool Equals(CompilerCacheKey other)
        {
            if (other == null || other._hashCode != _hashCode)
            {
                return false;
            }
            return _request.LayoutLocation == other._request.LayoutLocation
                && _request.ViewStartLocation == other._request.ViewStartLocation
                && _fileHash.SequenceEqual(other._fileHash);
        }
    }
}