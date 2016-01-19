using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    // Initially based on Path in Cake.Core.IO (http://cakebuild.net/)
    /// <summary>
    /// Provides properties and instance methods for working with paths.
    /// </summary>
    public abstract class Path
    {
        private static readonly char[] InvalidPathCharacters;

        static Path()
        {
            InvalidPathCharacters = System.IO.Path.GetInvalidPathChars().Concat(new[] { '*', '?' }).ToArray();
        }

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
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        protected Path(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty", nameof(path));
            }

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
            if (_path.StartsWith("/") && Segments.Length > 0)
            {
                Segments[0] = "/" + Segments[0];
            }

            // Validate the path.
            foreach (var character in path)
            {
                if (InvalidPathCharacters.Contains(character))
                {
                    const string format = "Illegal characters in directory path ({0})";
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, format, character), nameof(path));
                }
            }
        }
        
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this path.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => FullPath;

        public override bool Equals(object obj)
        {
            Path other = obj as Path;
            return other != null && GetType() == other.GetType() && other.FullPath == FullPath;
        }

        public override int GetHashCode() => (GetType() + FullPath).GetHashCode();

        protected string CollapsePath()
        {
            var stack = new Stack<string>();
            foreach (var segment in Segments)
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
