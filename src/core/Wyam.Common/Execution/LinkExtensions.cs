using System;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extensions to generate web links using global settings.
    /// </summary>
    public static class LinkExtensions
    {
        /// <summary>
        /// Gets a link for the root of the site using the host and root path specified in the settings.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>A link for the root of the site.</returns>
        public static string GetLink(this IExecutionContext context) =>
            GetLink(
                context,
                (NormalizedPath)null,
                context.String(Keys.Host),
                context.DirectoryPath(Keys.LinkRoot),
                context.Bool(Keys.LinksUseHttps),
                false,
                false);

        /// <summary>
        /// Gets a link for the specified metadata (typically a document) using the
        /// "RelativeFilePath" metadata value and the default settings from the
        /// <see cref="IReadOnlySettings" />. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="metadata">The metadata or document to generate a link for.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IExecutionContext context, IMetadata metadata, bool includeHost = false) =>
            GetLink(context, metadata, Keys.RelativeFilePath, includeHost);

        /// <summary>
        /// Gets a link for the specified metadata (typically a document) using the
        /// specified metadata value (by default, "RelativeFilePath") and the default settings from the
        /// <see cref="IReadOnlySettings" />. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="metadata">The metadata or document to generate a link for.</param>
        /// <param name="key">The key at which a <see cref="FilePath"/> can be found for generating the link.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IExecutionContext context, IMetadata metadata, string key, bool includeHost = false)
        {
            if (metadata?.ContainsKey(key) == true)
            {
                // Return the actual URI if it's absolute
                if (Uri.TryCreate(metadata.String(key), UriKind.Absolute, out Uri uri)
                    && (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                        || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
                {
                    return uri.ToString();
                }

                // Otherwise try to process the value as a file path
                FilePath filePath = metadata.FilePath(key);
                return filePath != null ? GetLink(context, filePath, includeHost) : null;
            }
            return null;
        }

        /// <summary>
        /// Converts the specified path into a string appropriate for use as a link using default settings from the
        /// <see cref="IReadOnlySettings" />. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IExecutionContext context, string path, bool includeHost = false) =>
            GetLink(
                context,
                path == null ? null : new FilePath(path),
                includeHost ? context.String(Keys.Host) : null,
                context.DirectoryPath(Keys.LinkRoot),
                context.Bool(Keys.LinksUseHttps),
                context.Bool(Keys.LinkHideIndexPages),
                context.Bool(Keys.LinkHideExtensions),
                context.Bool(Keys.LinkLowercase));

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the <see cref="IReadOnlySettings" />.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, "index.htm" and "index.html" file
        /// names will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(this IExecutionContext context, string path, string host, DirectoryPath root, bool useHttps, bool hideIndexPages, bool hideExtensions) =>
            GetLink(context, path == null ? null : new FilePath(path), host, root, useHttps, hideIndexPages, hideExtensions);

        /// <summary>
        /// Converts the specified path into a string appropriate for use as a link using default settings from the
        /// <see cref="IReadOnlySettings" />. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IExecutionContext context, NormalizedPath path, bool includeHost = false) =>
            GetLink(
                context,
                path,
                includeHost ? context.String(Keys.Host) : null,
                context.DirectoryPath(Keys.LinkRoot),
                context.Bool(Keys.LinksUseHttps),
                context.Bool(Keys.LinkHideIndexPages),
                context.Bool(Keys.LinkHideExtensions),
                context.Bool(Keys.LinkLowercase));

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the <see cref="IReadOnlySettings" />.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, "index.htm" and "index.html" file
        /// names will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionContext context,
            NormalizedPath path,
            string host,
            DirectoryPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions) =>
            GetLink(
                context,
                path,
                host,
                root,
                useHttps,
                hideIndexPages,
                hideExtensions,
                context.Bool(Keys.LinkLowercase));

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the <see cref="IReadOnlySettings" />.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, "index.htm" and "index.html" file
        /// names will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionContext context,
            NormalizedPath path,
            string host,
            DirectoryPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase) =>
            LinkGenerator.GetLink(
                path,
                host,
                root,
                useHttps ? "https" : null,
                hideIndexPages ? LinkGenerator.DefaultHidePages : null,
                hideExtensions ? LinkGenerator.DefaultHideExtensions : null,
                lowercase);
    }
}
