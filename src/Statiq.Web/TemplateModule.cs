using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web
{
    public class TemplateModule
    {
        public TemplateModule(string name, string mediaType, IModule module)
            : this(name, Config.FromDocument(doc => doc.MediaTypeEquals(mediaType)), module)
        {
            _ = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        }

        public TemplateModule(string name, Config<bool> condition, IModule module)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Condition = condition;
            Module = module;
        }

        public string Name { get; }

        /// <summary>
        /// A condition that determines whether this template should be applied to a given document.
        /// </summary>
        public Config<bool> Condition { get; set; }

        /// <summary>
        /// The template module to apply if <see cref="Condition"/> is <c>true</c>.
        /// </summary>
        public IModule Module { get; set; }
    }
}
