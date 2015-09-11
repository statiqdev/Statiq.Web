using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Download
{
    class DownloadInstruction
    {
        public Uri Uri { get; set; }

        public RequestHeader RequestHeader { get; set; }

        public bool ContainRequestHeader => RequestHeader != null;

        public DownloadInstruction()
        {

        }

        public DownloadInstruction(Uri uri)
        {
            Uri = uri;
        }

        public DownloadInstruction(Uri uri, RequestHeader requestHeader) : this(uri)
        {
            RequestHeader = requestHeader;
        }
    }
}
