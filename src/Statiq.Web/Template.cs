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
        public const string Markdown = nameof(Markdown);
        public const string Razor = nameof(Razor);
        public const string Handlebars = nameof(Handlebars);

        /// <summary>
        /// Creates a template definition that executes during the <see cref="Phase.PostProcess"/> phase.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="mediaType">The media type the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, string mediaType, IModule module)
            : this(name, Phase.PostProcess, mediaType, module)
        {
        }

        /// <summary>
        /// Creates a template definition that executes during the <see cref="Phase.PostProcess"/> phase.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="condition">The document condition the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, Config<bool> condition, IModule module)
            : this(name, Phase.PostProcess, condition, module)
        {
        }

        /// <summary>
        /// Creates a template definition.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="phase">The phase this template is executed (<see cref="Phase.Process"/> or <see cref="Phase.PostProcess"/>).</param>
        /// <param name="mediaType">The media type the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, Phase phase, string mediaType, IModule module)
            : this(name, phase, Config.FromDocument(doc => doc.MediaTypeEquals(mediaType)), module)
        {
            _ = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
        }

        /// <summary>
        /// Creates a template definition.
        /// </summary>
        /// <param name="name">The name of the template.</param>
        /// <param name="phase">The phase this template is executed (<see cref="Phase.Process"/> or <see cref="Phase.PostProcess"/>).</param>
        /// <param name="condition">The document condition the template applies to.</param>
        /// <param name="module">The template module to execute.</param>
        public Template(string name, Phase phase, Config<bool> condition, IModule module)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (phase != Phase.Process && phase != Phase.PostProcess)
            {
                throw new ArgumentException("Templates can only be executed in the process or post-process phase");
            }
            Phase = phase;
            Condition = condition;
            Module = module;
        }

        /// <summary>
        /// The name of this template.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The phase this template is executed (<see cref="Phase.Process"/> or <see cref="Phase.PostProcess"/>).
        /// </summary>
        public Phase Phase { get; }

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
