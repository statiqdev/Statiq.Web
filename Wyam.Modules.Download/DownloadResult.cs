using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Download
{
    public class DownloadResult
    {
        public Uri Uri { get; set; }

        public Stream Stream { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public DownloadResult()
        {

        }

        public DownloadResult(Uri uri, Stream stream, Dictionary<string, string> headers)
        {
            Uri = uri;
            Stream = stream;
            Headers = headers;
        }
    }
}
