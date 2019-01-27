using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// The request headers to use with the <see cref="Download"/> module.
    /// </summary>
    public class RequestHeaders
    {
        private readonly Dictionary<string, object> _headers = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new empty set of request headers.
        /// </summary>
        public RequestHeaders()
        {
        }

        /// <summary>
        /// Creates the specified request headers.
        /// </summary>
        /// <param name="headers">The request headers to create.</param>
        public RequestHeaders(IEnumerable<KeyValuePair<string, string>> headers)
        {
            _headers = headers.ToDictionary(x => x.Key, x => (object)x.Value);
        }

        /// <summary>
        /// Adds a new request header.
        /// </summary>
        /// <param name="name">The name of the request header to add.</param>
        /// <param name="value">The value of the request header to add.</param>
        /// <returns>The current instance.</returns>
        public RequestHeaders Add(string name, string value)
        {
            _headers.Add(name, value);
            return this;
        }

        /// <summary>
        /// Adds a new request header with multiple values.
        /// </summary>
        /// <param name="name">The name of the request header to add.</param>
        /// <param name="value">The value of the request header to add.</param>
        /// <returns>The current instance.</returns>
        public RequestHeaders Add(string name, IEnumerable<string> value)
        {
            _headers.Add(name, value);
            return this;
        }

        /// <summary>
        /// Removes a request header.
        /// </summary>
        /// <param name="name">The name of the request header to remove.</param>
        /// <returns>The current instance.</returns>
        public RequestHeaders Remove(string name)
        {
            _headers.Remove(name);
            return this;
        }

#pragma warning disable 1591
        public List<string> Accept { get; set; } = new List<string>();

        public List<string> AcceptCharset { get; set; } = new List<string>();

        public List<string> AcceptEncoding { get; set; } = new List<string>();

        public List<string> AcceptLanguage { get; set; } = new List<string>();

        public Tuple<string, string> BasicAuthorization { get; private set; }

        public List<string> Connection { get; set; } = new List<string>();

        public DateTimeOffset? Date { get; set; }

        public List<string> Expect { get; set; } = new List<string>();

        public bool? ExpectContinue { get; set; }

        public string From { get; set; }

        public string Host { get; set; }

        public List<string> IfMatch { get; set; } = new List<string>();

        public DateTimeOffset? IfModifiedSince { get; set; }

        public List<string> IfNoneMatch { get; set; } = new List<string>();

        public DateTimeOffset? IfUnmodifiedSince { get; set; }

        public int? MaxForwards { get; set; }

        public Uri Referrer { get; set; }

        public List<string> TransferEncoding { get; set; } = new List<string>();

        public bool? TransferEncodingChunked { get; set; }

        public void SetBasicAuthorization(string username, string password)
        {
            BasicAuthorization = Tuple.Create(username, password);
        }
#pragma warning restore 1591

        internal void ApplyTo(HttpRequestHeaders request)
        {
            foreach (string a in Accept)
            {
                request.Accept.Add(new MediaTypeWithQualityHeaderValue(a));
            }

            foreach (string a in AcceptCharset)
            {
                request.AcceptCharset.Add(new StringWithQualityHeaderValue(a));
            }

            foreach (string a in AcceptEncoding)
            {
                request.AcceptEncoding.Add(new StringWithQualityHeaderValue(a));
            }

            foreach (string a in AcceptLanguage)
            {
                request.AcceptLanguage.Add(new StringWithQualityHeaderValue(a));
            }

            if (BasicAuthorization != null)
            {
                Tuple<string, string> auth = BasicAuthorization;

                request.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{auth.Item1}:{auth.Item2}")));
            }

            foreach (string c in Connection)
            {
                request.Connection.Add(c);
            }

            if (Date.HasValue)
            {
                request.Date = Date;
            }

            foreach (string e in Expect)
            {
                request.Expect.Add(new NameValueWithParametersHeaderValue(e));
            }

            if (ExpectContinue.HasValue)
            {
                request.ExpectContinue = ExpectContinue;
            }

            if (!string.IsNullOrWhiteSpace(From))
            {
                request.From = From;
            }

            if (!string.IsNullOrWhiteSpace(Host))
            {
                request.Host = Host;
            }

            foreach (string i in IfMatch)
            {
                request.IfMatch.Add(new EntityTagHeaderValue(i));
            }

            if (IfModifiedSince.HasValue)
            {
                request.IfModifiedSince = IfModifiedSince;
            }

            foreach (string i in IfNoneMatch)
            {
                request.IfNoneMatch.Add(new EntityTagHeaderValue(i));
            }

            if (IfUnmodifiedSince.HasValue)
            {
                request.IfUnmodifiedSince = IfUnmodifiedSince;
            }

            if (MaxForwards.HasValue)
            {
                request.MaxForwards = MaxForwards;
            }

            if (Referrer != null)
            {
                request.Referrer = Referrer;
            }

            foreach (string t in TransferEncoding)
            {
                request.TransferEncoding.Add(new TransferCodingHeaderValue(t));
            }

            if (TransferEncodingChunked.HasValue)
            {
                request.TransferEncodingChunked = TransferEncodingChunked;
            }

            foreach (KeyValuePair<string, object> header in _headers)
            {
                IEnumerable<string> values = header.Value as IEnumerable<string>;
                if (values != null)
                {
                    request.Add(header.Key, values);
                }
                else
                {
                    request.Add(header.Key, header.Value.ToString());
                }
            }
        }
    }
}
