using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing front matter and sidecar files.
    /// </summary>
    public class ProcessMetadata : ForAllDocuments
    {
        public ProcessMetadata()
            : base(
                new ExtractFrontMatter(new ParseYaml()))
        {
        }
    }
}
