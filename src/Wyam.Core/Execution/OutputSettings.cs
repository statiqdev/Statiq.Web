using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Core.Execution
{
    internal class OutputSettings : IOutputSettings
    {
        private string _linkHost = null;

        public OutputSettings()
        {
            LinkHideIndexPages = true;
            LinkHideWebExtensions = true;
        }

        public string LinkHost
        {
            get { return _linkHost; }
            set
            {
                if (value == null)
                {
                    _linkHost = null;
                }
                if (Uri.CheckHostName(value) == UriHostNameType.Unknown)
                {
                    throw new ArgumentException("Value must be a valid hostname");
                }
                _linkHost = value;
            }
        }

        public DirectoryPath LinkRoot { get; set; }

        public bool LinkHideIndexPages { get; set; }

        public bool LinkHideWebExtensions { get; set; }
    }
}
