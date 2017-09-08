using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Modules;

namespace Wyam.Core.Modules.IO
{
    public abstract class ReadWebData : ReadDataModule<ReadWebData, object>
    {
        private readonly List<KeyValuePair<string, string>> _querystringArguments = new List<KeyValuePair<string, string>>();
        private readonly List<KeyValuePair<string, string>> _httpHeaders = new List<KeyValuePair<string, string>>();
        private NetworkCredential _credentials;
        private string _method = "GET";

        // Adds a querystring argument to be added to the URL prior to the HTTP call
        public ReadWebData AddQuerystringArgument(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            _querystringArguments.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        // Adds an HTTP header to the request prior to the HTTP call
        public ReadWebData AddHttpHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(nameof(key));
            }

            _httpHeaders.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        // Adds a set of network credentials added to the request prior to the HTTP call
        public ReadWebData WithCredentials(NetworkCredential credentials)
        {
            _credentials = credentials;
            return this;
        }

        // Adds a username and password to the request prior to the HTTP call
        public ReadWebData WithBasicAuth(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException(nameof(username));
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }

            _credentials = new NetworkCredential(username, password);
            return this;
        }

        public ReadWebData WithMethod(string method)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentException(nameof(method));
            }

            _method = method;
            return this;
        }

        protected HttpWebResponse GetResponse(string url)
        {
            // Add query string arguments
            UriBuilder builder = new UriBuilder(url);
            if (_querystringArguments.Any())
            {
                string query = builder.Query;
                if (string.IsNullOrEmpty(query))
                {
                    query = "?";
                }
                else
                {
                    query = query + "&";
                }
                query = query
                    + string.Join(
                        "&",
                        _querystringArguments.Select(x => string.IsNullOrEmpty(x.Value)
                            ? WebUtility.UrlEncode(x.Key)
                            : $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
                builder.Query = query;
            }

            // Configure the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.Uri);
            request.Method = _method;
            request.Credentials = _credentials;
            _httpHeaders.ForEach(h => request.Headers.Add(h.Key, h.Value));

            // Interpret the response
            return (HttpWebResponse)request.GetResponse();
        }
    }
}
