using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class NormalizedPath : IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        /// <summary>
        /// The default file provider.
        /// </summary>
        public static readonly Uri DefaultProvider = new Uri("file:///", UriKind.Absolute);

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get; }

        /// <summary>
        /// Gets a value indicating whether this path is relative.
        /// </summary>
        /// <value>
        /// <c>true</c> if this path is relative; otherwise, <c>false</c>.
        /// </value>
        public bool IsRelative => !IsAbsolute;

        /// <summary>
        /// Gets or sets a value indicating whether this path is absolute.
        /// </summary>
        /// <value>
        /// <c>true</c> if this path is absolute; otherwise, <c>false</c>.
        /// </value>
        public bool IsAbsolute { get; }

        /// <summary>
        /// Gets the segments making up the path.
        /// </summary>
        /// <value>The segments making up the path.</value>
        public string[] Segments { get; }

        /// <summary>
        /// Gets the provider for this path. If this is a relative path,
        /// the provider will always be <c>null</c>. If this is an absolute
        /// path and the provider is <c>null</c> it indicates the path
        /// is not intended for use with an actual file provider.
        /// </summary>
        /// <value>
        /// The provider for this path.
        /// </value>
        public Uri Provider { get; }

        /// <summary>
        /// Gets the root of this path or "." if this is a relative path
        /// or there is no root.
        /// </summary>
        /// <value>
        /// The root of this path.
        /// </value>
        public DirectoryPath Root
        {
            get
            {
                string directory = IsAbsolute ? System.IO.Path.GetPathRoot(FullPath) : ".";
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = ".";
                }
                return new DirectoryPath(Provider, directory);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        protected NormalizedPath(string path, bool? absolute)
            : this(GetProviderAndPath(path), absolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="provider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        protected NormalizedPath(Uri provider, string path, bool? absolute)
            : this(Tuple.Create(provider, path), absolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class. The new path
        /// will be absolute if the specified URI is absolute, otherwise it will be relative.
        /// </summary>
        /// <param name="path">The path as a URI.</param>
        protected NormalizedPath(Uri path)
            : this(GetProviderAndPath(path.ToString()), path.IsAbsoluteUri)
        {
        }

        /// <summary>
        /// Gets the provider and path from a path string. Implemented as a static
        /// so it can be used in a constructor chain.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The provider (item 1) and path (item 2).</returns>
        internal static Tuple<Uri, string> GetProviderAndPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Uri provider = DefaultProvider;
            
            // Did we get a delimiter?
            int delimiterIndex = path.IndexOf("|", StringComparison.Ordinal);
            if (delimiterIndex != -1)
            {
                // Path contains a provider delimiter, try to parse the provider
                string pathProvider = path.Substring(0, delimiterIndex);
                path = path.Length == delimiterIndex + 1 ? string.Empty : path.Substring(delimiterIndex + 1);
                if (!Uri.TryCreate(pathProvider, UriKind.Absolute, out provider))
                {
                    // Couldn't create the provider as a URI, try it as just a scheme
                    if (Uri.CheckSchemeName(pathProvider))
                    {
                        provider = new Uri($"{pathProvider}:///", UriKind.Absolute);
                    }
                    else
                    {
                        throw new ArgumentException("The provider URI is not valid");
                    }
                }
            }
            else if (Uri.TryCreate(path, UriKind.Absolute, out provider))
            {
                // No delimiter, but the path itself is a URI
                provider = new Uri(provider.GetLeftPart(UriPartial.Authority));
                path = provider.PathAndQuery + provider.Fragment;
            }

            return Tuple.Create(provider, path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="providerAndPath">The provider and path as a Tuple so it can
        /// be passed from both of the other constructors.</param>
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        private NormalizedPath(Tuple<Uri, string> providerAndPath, bool? absolute)
        {
            Uri provider = providerAndPath.Item1;
            string path = providerAndPath.Item2;

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty", nameof(path));
            }

            FullPath = path.Replace('\\', '/').Trim();

            // Remove relative part of a path, but only if it's not the only part
            if (FullPath.StartsWith("./", StringComparison.Ordinal) && FullPath.Length > 2)
            {
                FullPath = FullPath.Substring(2);
            }

            // Remove trailing slashes (as long as this isn't just a slash)
            if (FullPath.Length > 1)
            {
                FullPath = FullPath.TrimEnd('/');
            }

#if !UNIX
            if (FullPath.EndsWith(":", StringComparison.OrdinalIgnoreCase))
            {
                FullPath = string.Concat(FullPath, "/");
            }
#endif

            // Absolute path?
            IsAbsolute = absolute ?? System.IO.Path.IsPathRooted(FullPath);

            // Set provider (but only if absolute)
            if (IsRelative && provider != null)
            {
                throw new ArgumentException("Can not specify provider for relative paths", nameof(provider));
            }
            if (provider != null && !provider.IsAbsoluteUri)
            {
                throw new ArgumentException("The provider URI must always be absolute");
            }
            Provider = provider;

            // Extract path segments.
            Segments = FullPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                if (segment == ".")
                {
                    continue;
                }
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
            string collapsed = string.Join("/", stack.Reverse());
            return collapsed == string.Empty ? "." : collapsed;
        }

        public override int GetHashCode()
        {
            return FullPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NormalizedPath other = obj as NormalizedPath;

            // Special case for string, attempt to create like-typed path from the value
            if (other == null && obj is string)
            {
                if (this is FilePath)
                {
                    other = new FilePath((string)obj);
                }
                else if (this is DirectoryPath)
                {
                    other = new DirectoryPath((string)obj);
                }
            }

            return other != null && ((IEquatable<NormalizedPath>)this).Equals(other);
        }

        bool IEquatable<NormalizedPath>.Equals(NormalizedPath other)
        {
            if (IsAbsolute && other.IsAbsolute && Provider.Equals(other.Provider))
            {
                return false;
            }
            return FullPath.Equals(other.FullPath);
        }

        public int CompareTo(object obj)
        {
            NormalizedPath path = obj as NormalizedPath;
            return path == null ? 1 : CompareTo(path);
        }

        public int CompareTo(NormalizedPath other)
        {
            if (other == null)
            {
                return 1;
            }

            if (GetType() != other.GetType())
            {
                throw new ArgumentException("Paths are not the same type");
            }
            
            int providerCompare = string.Compare(Provider?.ToString(), other.Provider?.ToString(), StringComparison.Ordinal);
            return providerCompare == 0
                ? string.Compare(FullPath, other.FullPath, StringComparison.Ordinal)
                : providerCompare;
        }
    }
}
