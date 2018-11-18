using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class NormalizedPath : IComparable<NormalizedPath>, IComparable, IEquatable<NormalizedPath>
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        private const string FileProviderDelimiter = "|";

        /// <summary>
        /// The default file provider.
        /// </summary>
        public static readonly Uri DefaultFileProvider = new Uri("file:///", UriKind.Absolute);

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(string path, PathKind pathKind)
            : this(GetFileProviderAndPath(null, path), false, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="fileProvider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(string fileProvider, string path, PathKind pathKind)
            : this(GetFileProviderUri(fileProvider), path, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class
        /// with the given provider.
        /// </summary>
        /// <param name="fileProvider">The provider for this path.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        protected NormalizedPath(Uri fileProvider, string path, PathKind pathKind)
            : this(Tuple.Create(fileProvider, path), true, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class. The new path
        /// will be absolute if the specified URI is absolute, otherwise it will be relative.
        /// </summary>
        /// <param name="path">The path as a URI.</param>
        protected NormalizedPath(Uri path)
            : this(GetFileProviderAndPath(path, null), false, path.IsAbsoluteUri ? PathKind.Absolute : PathKind.Relative)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedPath" /> class.
        /// </summary>
        /// <param name="providerAndPath">The provider and path as a Tuple so it can
        /// be passed from both of the other constructors.</param>
        /// <param name="fullySpecified">If set to <c>true</c> indicates that this constructor was
        /// called from one where the provider and path were fully specified (as opposed to being inferred).</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        private NormalizedPath(Tuple<Uri, string> providerAndPath, bool fullySpecified, PathKind pathKind)
        {
            if (providerAndPath.Item2 == null)
            {
                throw new ArgumentNullException();
            }
            if (string.IsNullOrWhiteSpace(providerAndPath.Item2))
            {
                throw new ArgumentException("Path cannot be empty");
            }

            // Leave spaces since they're valid path chars
            FullPath = providerAndPath.Item2.Replace('\\', '/').Trim('\r', '\n', '\t');

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && FullPath.EndsWith(":", StringComparison.OrdinalIgnoreCase))
            {
                FullPath = string.Concat(FullPath, "/");
            }

            // Absolute path?
            switch (pathKind)
            {
                case PathKind.RelativeOrAbsolute:
                    IsAbsolute = System.IO.Path.IsPathRooted(FullPath);
                    break;
                case PathKind.Absolute:
                    IsAbsolute = true;
                    break;
                case PathKind.Relative:
                    IsAbsolute = false;
                    break;
            }

            // Set provider (but only if absolute)
            if (IsRelative && providerAndPath.Item1 != null)
            {
                throw new ArgumentException("Can not specify provider for relative paths");
            }
            if (providerAndPath.Item1 == null && IsAbsolute && !fullySpecified)
            {
                FileProvider = DefaultFileProvider;
            }
            else if (providerAndPath.Item1 != null && !providerAndPath.Item1.IsAbsoluteUri)
            {
                throw new ArgumentException("The provider URI must always be absolute");
            }
            else
            {
                FileProvider = providerAndPath.Item1;
            }

            // Extract path segments.
            Segments = FullPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Internal for testing
        internal static Uri GetFileProviderUri(string provider)
        {
            if (string.IsNullOrEmpty(provider))
            {
                return null;
            }

            // The use of IsWellFormedOriginalString() weeds out cases where a file system path was implicitly converted to a URI
            Uri uri;
            if (!Uri.TryCreate(provider, UriKind.Absolute, out uri) || !uri.IsWellFormedOriginalString())
            {
                // Couldn't create the provider as a URI, try it as just a scheme
                if (Uri.CheckSchemeName(provider))
                {
                    uri = new Uri($"{provider}:", UriKind.Absolute);
                }
                else
                {
                    throw new ArgumentException("The provider URI is not valid");
                }
            }
            return uri;
        }

        /// <summary>
        /// Gets the provider and path from a path string. Implemented as a static
        /// so it can be used in a constructor chain. Internal for testing.
        /// </summary>
        /// <param name="uriPath">The URI-based path.</param>
        /// <param name="stringPath">The string-based path.</param>
        /// <returns>The provider (item 1) and path (item 2).</returns>
        internal static Tuple<Uri, string> GetFileProviderAndPath(Uri uriPath, string stringPath)
        {
            if (uriPath != null && stringPath != null)
            {
                throw new ArgumentException($"{nameof(uriPath)} and {nameof(stringPath)} can not both have values");
            }

            // If we got a relative URI, then just use that as the path
            if (uriPath != null && !uriPath.IsAbsoluteUri)
            {
                return new Tuple<Uri, string>(null, uriPath.ToString());
            }

            // Did we get a delimiter?
            string path = stringPath;
            if (uriPath != null)
            {
                path = uriPath.ToString();
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            int delimiterIndex = path.IndexOf(FileProviderDelimiter, StringComparison.Ordinal);
            if (delimiterIndex != -1)
            {
                // Path contains a provider delimiter, try to parse the provider
                return Tuple.Create(
                    GetFileProviderUri(path.Substring(0, delimiterIndex)),
                    path.Length == delimiterIndex + 1 ? string.Empty : path.Substring(delimiterIndex + 1));
            }

            // See if the path is a URI and attempt to split it into left (provider) and right (path) parts
            // The use of IsWellFormedOriginalString() weeds out cases where a file system path was implicitly converted to a URI
            Uri fileProvider = uriPath;
            if (fileProvider != null || (Uri.TryCreate(stringPath, UriKind.Absolute, out fileProvider) && fileProvider.IsWellFormedOriginalString()))
            {
                // No delimiter, but the path itself is a URI
                // However, if there is no "right part" go back to treating the whole thing as a path
                string rightPart = GetRightPart(fileProvider);
                if (!string.IsNullOrEmpty(rightPart))
                {
                    return Tuple.Create(
                        new Uri(GetLeftPart(fileProvider), UriKind.Absolute),
                        rightPart);
                }
            }

            return Tuple.Create(uriPath, stringPath);
        }

        private static string GetLeftPart(Uri uri) =>
            uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.PathAndQuery & ~UriComponents.Fragment, UriFormat.Unescaped);

        private static string GetRightPart(Uri uri) =>
            uri.GetComponents(UriComponents.PathAndQuery | UriComponents.Fragment, UriFormat.Unescaped);

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
        /// Gets the file provider for this path. If this is a relative path,
        /// the file provider will always be <c>null</c>. If this is an absolute
        /// path and the file provider is <c>null</c> it indicates the path
        /// is not intended for use with an actual file provider.
        /// </summary>
        /// <value>
        /// The file provider for this path.
        /// </value>
        public Uri FileProvider { get; }

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
                return new DirectoryPath(FileProvider, directory);
            }
        }

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

        /// <summary>
        /// Returns a <see cref="string" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (IsRelative || FileProvider == null)
            {
                return FullPath;
            }
            string rightPart = GetRightPart(FileProvider);
            if (string.IsNullOrEmpty(rightPart) || rightPart == "/")
            {
                // Remove the proceeding slash from FullPath if the provider already has one
                return FileProvider + (rightPart == "/" && FullPath.StartsWith("/") ? FullPath.Substring(1) : FullPath);
            }
            return FileProvider + FileProviderDelimiter + FullPath;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 31) + (FileProvider?.GetHashCode() ?? 0);
            hash = (hash * 31) + FullPath.GetHashCode();
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            NormalizedPath other = obj as NormalizedPath;

            // Special case for string, attempt to create like-typed path from the value
            string path = obj as string;
            if (other == null && path != null)
            {
                if (this is FilePath)
                {
                    other = new FilePath(path);
                }
                else if (this is DirectoryPath)
                {
                    other = new DirectoryPath(path);
                }
            }

            return other != null && ((IEquatable<NormalizedPath>)this).Equals(other);
        }

        bool IEquatable<NormalizedPath>.Equals(NormalizedPath other) =>
            other != null
            && FileProvider?.ToString() == other.FileProvider?.ToString()
            && FullPath == other.FullPath;

        /// <inheritdoc />
        public int CompareTo(object obj)
        {
            NormalizedPath path = obj as NormalizedPath;
            return path == null ? 1 : CompareTo(path);
        }

        /// <inheritdoc />
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

            int providerCompare = string.Compare(FileProvider?.ToString(), other.FileProvider?.ToString(), StringComparison.Ordinal);
            return providerCompare == 0
                ? string.Compare(FullPath, other.FullPath, StringComparison.Ordinal)
                : providerCompare;
        }
    }
}
