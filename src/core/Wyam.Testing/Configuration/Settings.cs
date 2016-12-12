using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.IO;

namespace Wyam.Testing.Configuration
{
    public class Settings : ISettings
    {
        public string Host { get; set; }

        public bool LinksUseHttps { get; set; }

        public DirectoryPath LinkRoot { get; set; }

        public bool LinkHideIndexPages { get; set; }

        public bool LinkHideExtensions { get; set; }

        public bool UseCache { get; set; }

        public bool CleanOutputPath { get; set; }
    }
}
