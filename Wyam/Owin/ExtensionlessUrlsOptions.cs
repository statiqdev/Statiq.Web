using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.Infrastructure;

namespace Wyam.Owin
{
    public class ExtensionlessUrlsOptions
    {
        public ExtensionlessUrlsOptions()
        {
            // Prioritized list
            DefaultExtensions = new List<string>()
            {
                ".htm",
                ".html"
            };
        }

        public IList<string> DefaultExtensions { get; set; }


        public IFileSystem FileSystem { get; set; }
    }
}
