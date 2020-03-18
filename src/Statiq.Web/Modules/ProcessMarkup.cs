using System;
using System.Collections.Generic;
using System.Text;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing markup languages like Markdown.
    /// </summary>
    public class ProcessMarkup : ParentModule
    {
        public ProcessMarkup()
            : base(
                new ProcessShortcodes("!"),
                new ExecuteIf(Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.Markdown)))
                {
                    new RenderMarkdown().UseExtensions()
                })
        {
        }
    }
}
