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
    public abstract class NormalizedPath : IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        /// <summary>
        /// Use this provider name to indicate that the path is not intended for use with an actual file provider.
        /// For example, as the source for documents generated on the fly by a module.
        /// </summary>
        public static readonly string AbstractProvider = "abstract";

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
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        protected NormalizedPath(string path, bool? absolute)
            : this(GetProviderAndPath(path), absolute)
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

            string provider = null;
            int providerIndex = path.IndexOf("::", StringComparison.Ordinal);
            if (providerIndex != -1)
            {
                // Return a null provider if the :: was used as an escape without an actual provider
                provider = providerIndex == 0 ? null : path.Substring(0, providerIndex);
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
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        protected NormalizedPath(string provider, string path, bool? absolute)
            : this(Tuple.Create(provider, path), absolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="providerAndPath">The provider and path as a Tuple so it can
        /// be passed from both of the other constructors.</param>
        /// <param name="absolute">Indicates if the path should be explicitly considered absolute.</param>
        private NormalizedPath(Tuple<string, string> providerAndPath, bool? absolute)
        {
            string provider = providerAndPath.Item1;
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
            if (IsRelative)
            {
                if (!string.IsNullOrEmpty(provider))
                {
                    throw new ArgumentException("Can not specify provider for relative paths", nameof(provider));
                }

                // If the provider is the default provider, set to null for relative paths
                provider = null;
            }
            else if (provider == null)
            {
                // Use string.Empty as the default provider for absolute paths
                provider = string.Empty;
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
            if (IsAbsolute && other.IsAbsolute && Provider != other.Provider)
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
            
            int providerCompare = string.Compare(Provider, other.Provider, StringComparison.Ordinal);
            return providerCompare == 0
                ? string.Compare(FullPath, other.FullPath, StringComparison.Ordinal)
                : providerCompare;
        }

        /// <summary>
        /// Converts the path into a string appropriate for use as a link.
        /// </summary>
        /// <param name="root">The root of the link, which could be a web URL such as
        /// "http://foo.com" or "http://foo.com/bar", a relative path such as "/foo/bar",
        /// or any other string. The value of this parameter is prepended to the link text
        /// and separated with a slash. If this parameter is <c>null</c>, the link text
        /// is returned as-is and no initial slash is inferred.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, "index.htm" and "index.html" file
        /// names will be hidden.</param>
        /// <param name="hideWebExtensions">If set to <c>true</c>, extensions ending in ".htm" or
        /// ".html" will be hidden.</param>
        /// <returns>A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.</returns>
        public string ToLink(string root = "/", bool hideIndexPages = true, bool hideWebExtensions = true)
        {
            // Remove index pages and extensions if a file path
            NormalizedPath path = this;
            FilePath filePath = path as FilePath;
            if (filePath != null)
            {
                if (hideIndexPages && (filePath.FileName.FullPath == "index.htm" || filePath.FileName.FullPath == "index.html"))
                {
                    path = filePath.Directory;
                }
                else if (hideWebExtensions && (filePath.Extension == ".htm" || filePath.Extension == ".html"))
                {
                    path = filePath.ChangeExtension(null);
                }
            }

            // Get the link
            string link = Collapse(path);
            if (string.IsNullOrWhiteSpace(link) || link == ".")
            {
                return "/";
            }
            if (root != null && link.StartsWith("/"))
            {
                link = link.Substring(1);
            }

            // Combine with the root
            if (root == null)
            {
                root = string.Empty;
            }
            else if (!root.EndsWith("/"))
            {
                root += "/";
            }

            return root + link;
        }
    }
}
