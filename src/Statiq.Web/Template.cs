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
    /// <summary>
    /// Represents a "template" (shorthand for "template language" or "template engine").
    /// </summary>
    public class Template
    {
        /// <summary>
        /// Creates a template definition.
        /// </summary>
        /// <param name="templateType">The type of the template (when it will be applied).</param>
        /// <param name="module">The template module to execute.</param>
        public Template(TemplateType templateType, IModule module)
        {
            TemplateType = templateType;
            Module = module;
        }

        /// <summary>
        /// The type of this template (when it will be applied).
        /// </summary>
        public TemplateType TemplateType { get; }

        /// <summary>
        /// The template module to apply if the media types match.
        /// </summary>
        public IModule Module { get; set; }
    }
}
