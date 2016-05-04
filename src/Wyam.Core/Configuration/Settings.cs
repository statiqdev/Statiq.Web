using System;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common.Configuration;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Core.Configuration
{
    internal class Settings : ISettings
    {
        private string _host = null;

        public Settings()
        {
            LinkHideIndexPages = true;
            LinkHideExtensions = true;
        }

        public string Host
        {
            get { return _host; }
            set
            {
                if (value == null)
                {
                    _host = null;
                }
                if (Uri.CheckHostName(value) == UriHostNameType.Unknown)
                {
                    throw new ArgumentException("Value must be a valid hostname");
                }
                _host = value;
            }
        }

        public DirectoryPath LinkRoot { get; set; }

        public bool LinkHideIndexPages { get; set; }

        public bool LinkHideExtensions { get; set; }

        public bool UseCache { get; set; } = true;

        public bool CleanOutputPath { get; set; } = true;
    }
}
