using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Sets a title metadata key for documents based on their file path or source.
    /// </summary>
    /// <remarks>
    /// This will split the title at special characters, capitalize first letters, remove extensions, etc.
    /// </remarks>
    /// <metadata name="Title" type="string">The title of the document.</metadata>
    /// <category>Metadata</category>
    public class Title : IModule
    {
        private readonly DocumentConfig _title = GetTitle;
        private string _key = Keys.Title;
        private bool _keepExisting = true;

        /// <summary>
        /// This will use the existing title metadata key if one exists,
        /// otherwise it will set a title based on the document source
        /// or the RelativeFilePath key if no source is available.
        /// </summary>
        public Title()
        {
        }

        /// <summary>
        /// This sets the title of all input documents to the specified string.
        /// </summary>
        /// <param name="title">The title to set.</param>
        public Title(string title)
        {
            _title = (doc, ctx) => title;
        }

        /// <summary>
        /// This sets the title of all input documents to a value from the delegate.
        /// </summary>
        /// <param name="title">A delegate that must return a string.</param>
        public Title(ContextConfig title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            _title = (doc, ctx) => title(ctx);
        }

        /// <summary>
        /// This sets the title of all input documents to a value from the delegate.
        /// </summary>
        /// <param name="title">A delegate that must return a string.</param>
        public Title(DocumentConfig title)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            _title = title;
        }

        /// <summary>
        /// Specifies the key to set for the title. By default this module sets
        /// a value for the key Title.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        public Title WithKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(key));
            }

            _key = key;
            return this;
        }

        /// <summary>
        /// Indicates that an existing value in the title key should be kept. The
        /// default value is <c>true</c>. Setting to <c>false</c> will always
        /// set the title metadata to the result of this module, even if the
        /// result is null or empty.
        /// </summary>
        /// <param name="keepExisting">Whether to keep the existing title metadata value.</param>
        public Title KeepExisting(bool keepExisting = true)
        {
            _keepExisting = keepExisting;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(input =>
                {
                    // Check if there's already a title set
                    if (_keepExisting && input.WithoutSettings.ContainsKey(_key))
                    {
                        return input;
                    }

                    // Calculate the new title
                    string title = _title.Invoke<string>(input, context);
                    return title == null 
                        ? input 
                        : context
                            .GetDocument(input, new MetadataItems
                            {
                                {_key, title}
                            });
                });
        }

        public static object GetTitle(IDocument doc, IExecutionContext context)
        {
            FilePath path = doc.Source ?? doc.FilePath(Keys.RelativeFilePath);
            return path == null ? null : GetTitle(path);
        }

        public static string GetTitle(FilePath path)
        {
            // Get the filename, unless an index file, then get containing directory
            string title = path.Segments.Last();
            if (title.StartsWith("index.") && path.Segments.Length > 1)
            {
                title = path.Segments[path.Segments.Length - 2];
            }

            // Strip the extension(s)
            int extensionIndex = title.IndexOf('.');
            if (extensionIndex > 0)
            {
                title = title.Substring(0, extensionIndex);
            }

            // Decode URL escapes
            title = WebUtility.UrlDecode(title);

            // Replace special characters with spaces
            title = title.Replace('-', ' ').Replace('_', ' ');

            // Join adjacent spaces
            while (title.IndexOf("  ", StringComparison.Ordinal) > 0)
            {
                title = title.Replace("  ", " ");
            }

            // Capitalize
            title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title);

            return title;
        }
    }
}