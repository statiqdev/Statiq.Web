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
        public RequestHeaders Headers { get; set; }

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
        public DownloadRequest(Uri uri) =>
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));

        /// <summary>
        /// Sets the request headers.
        /// </summary>
        /// <param name="headers">The request headers to set.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithHeaders(RequestHeaders headers)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            return this;
        }

        /// <summary>
        /// Sets a query string value.
        /// </summary>
        /// <param name="name">The name of the query string parameter.</param>
        /// <param name="value">The value of the query string parameter.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithQueryString(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            QueryString[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the request method.
        /// </summary>
        /// <param name="method">The method to set.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithMethod(HttpMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            return this;
        }

        /// <summary>
        /// Sets the content of the request (only applicable to some request methods).
        /// </summary>
        /// <param name="content">The content to set.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithContent(HttpContent content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the string content of the request (only applicable to some request methods).
        /// </summary>
        /// <param name="content">The content to set.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithContent(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Content = new StringContent(content);
            return this;
        }

        /// <summary>
        /// Sets the credentials to use for the request.
        /// </summary>
        /// <param name="credentials">The credentials to use.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithCredentials(NetworkCredential credentials)
        {
            Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
            return this;
        }

        /// <summary>
        /// Sets the credentials to use for the request.
        /// </summary>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <returns>The current instance.</returns>
        public DownloadRequest WithCredentials(string userName, string password)
        {
            Credentials = new NetworkCredential(userName, password);
            return this;
        }
    }
}