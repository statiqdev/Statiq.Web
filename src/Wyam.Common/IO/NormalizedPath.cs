using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class NormalizedPath
    {
        private readonly string _path;

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath => _path;

        /// <summary>
        /// Gets a value indicating whether this path is relative.
        /// </summary>
        /// <value>
        /// <c>true</c> if this path is relative; otherwise, <c>false</c>.
        /// </value>
        public bool IsRelative { get; }

        /// <summary>
        /// Gets the segments making up the path.
        /// </summary>
        /// <value>The segments making up the path.</value>
        public string[] Segments { get; }

        /// <summary>
        /// Gets the provider for this path.
        /// </summary>
        /// <value>
        /// The provider for this path.
        /// </value>
        public string Provider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        protected NormalizedPath(string path)
            : this(GetProviderAndPath(path))
        {
        }

        /// <summary>
        /// Gets the provider and path from a path string. Implemented as a static
        /// so it can be used in a constructor chain.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The provider (item 1) and path (item 2).</returns>
        internal static Tuple<string, string> GetProviderAndPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string provider = string.Empty;
            int providerIndex = path.IndexOf("::", StringComparison.Ordinal);
            if (providerIndex != -1)
            {
                provider = path.Substring(0, providerIndex);
                path = path.Substring(providerIndex + 2);
            }
            return Tuple.Create(provider, path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="provider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        protected NormalizedPath(string provider, string path)
            : this(Tuple.Create(provider, path))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="providerAndPath">The provider and path as a Tuple so it can
        /// be passed from both of the other constructors.</param>
        private NormalizedPath(Tuple<string, string> providerAndPath)
        {
            string provider = providerAndPath.Item1;
            string path = providerAndPath.Item2;

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty", nameof(path));
            }

            Provider = provider;
            _path = path.Replace('\\', '/').Trim();
            _path = _path == "./" ? string.Empty : _path;

            // Remove relative part of a path.
            if (_path.StartsWith("./", StringComparison.Ordinal))
            {
                _path = _path.Substring(2);
            }

            // Remove trailing slashes.
            _path = _path.TrimEnd('/', '\\');

#if !UNIX
            if (_path.EndsWith(":", StringComparison.OrdinalIgnoreCase))
            {
                _path = string.Concat(_path, "/");
            }
#endif

            // Relative path?
            IsRelative = !System.IO.Path.IsPathRooted(_path);

            // Extract path segments.
            Segments = _path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => FullPath;

        internal static string Collapse(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            Stack<string> stack = new Stack<string>();
            string[] segments = path.FullPath.Split('/', '\\');
            foreach (string segment in segments)
            {
                if (segment == "..")
                {
                    if (stack.Count > 1)
                    {
                        stack.Pop();
                    }
                    continue;
                }
                stack.Push(segment);
            }
            return string.Join("/", stack.Reverse());
        }
    }
}
