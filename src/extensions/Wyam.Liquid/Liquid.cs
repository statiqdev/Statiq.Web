using System;
using System.Collections.Generic;
using System.Linq;
using DotLiquid;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Liquid
{
    /// <summary>
    /// Parses Liquid content and renders it to HTML.
    /// </summary>
    /// <remarks>
    /// Parses Liquid content in each input document and outputs documents with rendered HTML content.
    /// </remarks>
    /// <category>Templates</category>
    public class Liquid : IModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;

        /// <summary>
        /// Processes Markdown in the content of the document.
        /// </summary>
        public Liquid()
        {
        }

        /// <summary>
        /// Processes Markdown in the metadata of the document. The rendered HTML will be placed
        /// </summary>
        /// <param name="sourceKey">The metadata key of the Markdown to process.</param>
        /// <param name="destinationKey">The metadata key to store the rendered HTML (if null, it gets placed back in the source metadata key).</param>
        public Liquid(string sourceKey, string destinationKey = null)
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                Trace.Verbose(
                    "Processing Liquid markup {0} for {1}",
                    string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey),
                    input.SourceString());
                string result;
                IExecutionCache executionCache = context.ExecutionCache;

                if (!executionCache.TryGetValue<string>(input, _sourceKey, out result))
                {
                    if (string.IsNullOrEmpty(_sourceKey))
                    {
                        Template liquidTemplate = Template.Parse(input.Content);

                        // TODO: We need to pass in the MetaData
                        result = liquidTemplate.Render();
                    }
                    else if (input.ContainsKey(_sourceKey))
                    {
                        Template liquidTemplate = Template.Parse(input.String(_sourceKey) ?? string.Empty);

                        // TODO: We need to pass in the MetaData
                        result = liquidTemplate.Render();
                    }
                    else
                    {
                        // Don't do anything if the key doesn't exist
                        return input;
                    }

                    executionCache.Set(input, _sourceKey, result);
                }

                return string.IsNullOrEmpty(_sourceKey)
                    ? context.GetDocument(input, context.GetContentStream(result))
                    : context.GetDocument(input, new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    });
            });
        }

        /// <summary>
        /// Register ViewModels that implement a given Interface
        /// </summary>
        /// <param name="rootType">The type that will be registered with the Liquid rendering engine</param>
        public void RegisterViewModel(Type rootType)
        {
            // TODO: Update this to use a marker interface for registering types
            rootType
               .Assembly
               .GetTypes()
               .Where(t => t.Namespace == rootType.Namespace)
               .ToList()
               .ForEach(RegisterSafeTypeWithAllProperties);
        }

        /// <summary>
        /// Register the properties on a specific Type
        /// </summary>
        /// <param name="type">The type whose properties will be registered for use with the Liquid rendering engine</param>
        public void RegisterSafeTypeWithAllProperties(Type type)
        {
            Template.RegisterSafeType(
                type,
                type
                .GetProperties()
                .Select(p => p.Name)
                .ToArray());
        }
    }
}
