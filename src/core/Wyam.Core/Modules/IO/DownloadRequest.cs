using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// A download request for use with the <see cref="Download"/> module.
    /// </summary>
    public class DownloadRequest
    {
        /// <summary>
        /// The URI to download from.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Request headers.
        /// </summary>
        public RequestHeaders RequestHeaders { get; set; }

        /// <summary>
        /// The query string parameters. These will be combined with any that already exist in <see cref="Uri"/>.
        /// </summary>
        public IDictionary<string, string> QueryString { get; } = new Dictionary<string, string>();

        /// <summary>
        /// The method to use.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The content of the request (has no effect for some methods like GET).
        /// </summary>
        public HttpContent Content { get; set; }

        /// <summary>
        /// The network credentials to use for the request.
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        public DownloadRequest(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(uri));
            }
            Uri = new Uri(uri);
        }

        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        public DownloadRequest(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            Uri = uri;
        }
    }
}