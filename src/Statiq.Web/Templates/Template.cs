using System;
using Statiq.Common;

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
        /// <param name="contentType">The type of content this template applies to.</param>
        /// <param name="phase">The phase this template applies to (<see cref="Phase.Process"/> or <see cref="Phase.PostProcess"/>).</param>
        /// <param name="module">The template module to execute.</param>
        public Template(ContentType contentType, Phase phase, IModule module)
        {
            if (phase != Phase.Process && phase != Phase.PostProcess)
            {
                throw new ArgumentException($"Templates can only apply to the {Phase.Process} and {Phase.PostProcess} phases");
            }

            ContentType = contentType;
            Phase = phase;
            Module = module;
        }

        /// <summary>
        /// The type of content this template applies to.
        /// </summary>
        public ContentType ContentType { get; }

        public Phase Phase { get; }

        /// <summary>
        /// The template module to apply if the media types match.
        /// </summary>
        /// <remarks>
        /// This can be set to null in the event a template has already been added but you don't want it to do anything.
        /// </remarks>
        public IModule Module { get; set; }
    }
}
