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
        /// <param name="name">The name of the template.</param>
        /// <param name="templateType">The type of the template (when it will be applied).</param>
        /// <param name="mediaType">The media type the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, TemplateType templateType, string mediaType, IModule module)
            : this(name, templateType, new string[] { mediaType.ThrowIfNull(nameof(mediaType)) }, module)
        {
        }

        /// <summary>
        /// Creates a template definition.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="templateType">The type of the template (when it will be applied).</param>
        /// <param name="mediaTypes">The media types the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, TemplateType templateType, IEnumerable<string> mediaTypes, IModule module)
            : this(name, templateType, Config.FromDocument(doc => mediaTypes?.Any(m => doc.MediaTypeEquals(m)) == true), module)
        {
        }

        /// <summary>
        /// Creates a template definition.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="templateType">The type of the template (when it will be applied).</param>
        /// <param name="condition">The document condition the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, TemplateType templateType, Config<bool> condition, IModule module)
        {
            Name = name.ThrowIfNull(nameof(name));
            TemplateType = templateType;
            Condition = condition.ThrowIfNull(nameof(condition));
            Module = module;
        }

        /// <summary>
        /// The name of this template.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of this template (when it will be applied).
        /// </summary>
        public TemplateType TemplateType { get; }

        /// <summary>
        /// A condition that determines whether this template should be applied to a given document (usually based on media type).
        /// </summary>
        public Config<bool> Condition { get; set; }

        /// <summary>
        /// The template module to apply if <see cref="Condition"/> is <c>true</c>.
        /// </summary>
        public IModule Module { get; set; }
    }
}
