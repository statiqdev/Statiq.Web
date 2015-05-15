using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.Expiration.Interfaces;

namespace Wyam.Modules.Razor
{
    public class DocumentFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider _physicalFileProvider;
        private readonly string _content;

        public DocumentFileProvider(string root, string content)
        {
            _physicalFileProvider = new PhysicalFileProvider(root);
            _content = content;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            IFileInfo fileInfo = _physicalFileProvider.GetFileInfo(subpath);
            return new DocumentFileInfo(fileInfo, _content);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _physicalFileProvider.GetDirectoryContents(subpath);
        }

        public IExpirationTrigger Watch(string filter)
        {
            return _physicalFileProvider.Watch(filter);
        }

        private class DocumentFileInfo : IFileInfo
        {
            private readonly IFileInfo _info;
            private readonly string _content;

            public DocumentFileInfo(IFileInfo info, string content)
            {
                _info = info;
                _content = content;
            }

            public bool Exists
            {
                get { return _info.Exists; }
            }

            public long Length
            {
                get { return _info.Length; }
            }

            public string PhysicalPath
            {
                get { return _info.PhysicalPath; }
            }

            public string Name
            {
                get { return _info.Name; }
            }

            public DateTimeOffset LastModified
            {
                get { return _info.LastModified; }
            }

            public bool IsDirectory
            {
                get { return _info.IsDirectory; }
            }

            public Stream CreateReadStream()
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(_content));
;           }
        }
    }
}
