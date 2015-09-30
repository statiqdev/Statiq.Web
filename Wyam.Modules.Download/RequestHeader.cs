using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Download
{
    public class RequestHeader
    {
        public List<string> Accept { get; set; } = new List<string>();

        public List<string> AcceptCharset { get; set; } = new List<string>();

        public List<string> AcceptEncoding { get; set; } = new List<string>();

        public List<string> AcceptLanguage { get; set; } = new List<string>();

        Tuple<string, string> _basicAuthorization;

        public Tuple<string, string> BasicAuthorization
        {
            get
            {
                return _basicAuthorization;
            }
        }

        public List<string> Connection { get; set; } = new List<string>();

        public DateTimeOffset? Date;

        public List<string> Expect { get; set; } = new List<string>();

        public bool? ExpectContinue { get; set; }

        public string From { get; set; }

        public string Host { get; set; }

        public List<string> IfMatch { get; set; } = new List<string>();

        public DateTimeOffset? IfModifiedSince;

        public List<string> IfNoneMatch { get; set; } = new List<string>();

        public DateTimeOffset? IfUnmodifiedSince { get; set; }

        public int? MaxForwards { get; set; }

        public Uri Referrer { get; set; }

        public List<string> TransferEncoding { get; set; } = new List<string>();

        public bool? TransferEncodingChunked { get; set; }

        public void SetBasicAuthorization(string username, string password)
        {
            _basicAuthorization = Tuple.Create(username, password);
        }
    }
}
