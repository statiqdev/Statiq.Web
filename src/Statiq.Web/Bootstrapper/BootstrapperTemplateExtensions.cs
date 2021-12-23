using System;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperTemplateExtensions
    {
        /// <summary>
        /// Configures the set of template modules.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="action">The configuration action.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper ConfigureTemplates<TBootstrapper>(this TBootstrapper bootstrapper, Action<Templates> action)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureServices(services =>
                action(services

                    // We need to build an intermediate service provider to get access to the singleton (which is okay
                    // since it was registered as a singleton instance, not constructed by the service provider)
                    .BuildServiceProvider()
                    .GetRequiredService<Templates>()));

        /// <summary>
        /// Adds a new template.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="mediaType">The media type this template applies to.</param>
        /// <param name="contentType">The type of content this template applies to.</param>
        /// <param name="phase">The phase this template applies to (<see cref="Phase.Process"/> or <see cref="Phase.PostProcess"/>).</param>
        /// <param name="module">The template module to execute.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper AddTemplate<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string mediaType,
            ContentType contentType,
            Phase phase,
            IModule module)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureTemplates(templates => templates.Add(mediaType, new Template(contentType, phase, module)));

        /// <summary>
        /// Removes the template for a given media type.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="mediaType">The media type to remove the template for (if one exists).</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper RemoveTemplate<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string mediaType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureTemplates(templates => templates.Remove(mediaType));

        /// <summary>
        /// Modifies an existing template.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="mediaType">The media type of the template to modify.</param>
        /// <param name="modifyModule">An action that modifies the template module and returns it or a new module to use.</param>
        /// <returns>The bootstrapper.</returns>
        public static TBootstrapper ModifyTemplate<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string mediaType,
            Func<IModule, IModule> modifyModule)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureTemplates(templates =>
            {
                if (templates.TryGetValue(mediaType, out Template template))
                {
                    template.Module = modifyModule(template.Module);
                }
                else
                {
                    throw new Exception($"Template for media type {mediaType} not found");
                }
            });

        public static TBootstrapper SetDefaultLayoutTemplate<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string mediaType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureTemplates(templates =>
            {
                if (!templates.TryGetValue(mediaType, out Template template))
                {
                    throw new Exception($"Template for media type {mediaType} not found");
                }
                if (template.ContentType != ContentType.Content)
                {
                    throw new Exception($"Template for media type {mediaType} is not a {ContentType.Content} template");
                }
                if (template.Phase != Phase.PostProcess)
                {
                    throw new Exception($"Template for media type {mediaType} is not a {Phase.PostProcess} template");
                }
                templates[MediaTypes.HtmlFragment].Module = template.Module;
            });

        public static TBootstrapper SetDefaultLayoutModule<TBootstrapper>(
            this TBootstrapper bootstrapper,
            IModule module)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureTemplates(templates =>
            {
                templates[MediaTypes.HtmlFragment].Module = module;
            });
    }
}