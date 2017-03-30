using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;

namespace Wyam.Core.Modules.Metadata
{
    /// <summary>
    /// Adds the specified metadata to each input document.
    /// </summary>
    /// <category>Metadata</category>
    public class Meta : IModule
    {
        private readonly string _key;
        private readonly ConfigHelper<object> _metadata;
        private readonly IModule[] _modules;
        private bool _forEachDocument;
        private bool _ignoreNull;
        private bool _onlyIfNonExisting;

        /// <summary>
        /// The specified object is added as metadata for the specified key for every input document.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <param name="metadata">The object to add as metadata.</param>
        public Meta(string key, object metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
            _metadata = new ConfigHelper<object>(metadata);
        }

        /// <summary>
        /// Uses a function to determine an object to be added as metadata for each document.
        /// This allows you to specify different metadata for each document depending on the context.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <param name="metadata">A delegate that returns the object to add as metadata.</param>
        public Meta(string key, ContextConfig metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
            _metadata = new ConfigHelper<object>(metadata);
        }

        /// <summary>
        /// Uses a function to determine an object to be added as metadata for each document.
        /// This allows you to specify different metadata for each document depending on the input.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <param name="metadata">A delegate that returns the object to add as metadata.</param>
        public Meta(string key, DocumentConfig metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
            _metadata = new ConfigHelper<object>(metadata);
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and all metadata that exist in all of the result documents
        /// are added as metadata to each input document.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        public Meta(params IModule[] modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Specifies that the whole sequence of modules should be executed for every input document
        /// (as opposed to the default behavior of the sequence of modules only being executed once
        /// with an empty initial document). This method has no effect if no modules are specified.
        /// </summary>
        /// <param name="forEachDocument"><c>true</c> to execute for every input document.</param>
        /// <returns>The current module instance.</returns>
        public Meta ForEachDocument(bool forEachDocument = true)
        {
            _forEachDocument = forEachDocument;
            return this;
        }

        /// <summary>
        /// Ignores null values and does not add a metadata item for them.
        /// </summary>
        /// <param name="ignoreNull"><c>true</c> to ignore null values.</param>
        /// <returns>The current module instance.</returns>
        public Meta IgnoreNull(bool ignoreNull = true)
        {
            _ignoreNull = ignoreNull;
            return this;
        }

        /// <summary>
        /// Only sets the new metadata value if a value doesn't already exist.
        /// The default behavior is to set the new value regardless.
        /// </summary>
        /// <param name="onlyIfNonExisting"><c>true</c> if the new value should only be set if it doesn't already exist.</param>
        /// <returns>The current module instance.</returns>
        public Meta OnlyIfNonExisting(bool onlyIfNonExisting = true)
        {
            _onlyIfNonExisting = onlyIfNonExisting;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();

                // Execute the modules for each input document
                if (_forEachDocument)
                {
                    return inputs.Select(context, input =>
                    {
                        foreach (IDocument result in context.Execute(_modules, new[] { input }))
                        {
                            foreach (KeyValuePair<string, object> kvp in result)
                            {
                                if (kvp.Value != null || !_ignoreNull)
                                {
                                    metadata[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                        return context.GetDocument(input,
                            _onlyIfNonExisting ? metadata.Where(x => !input.ContainsKey(x.Key)) : metadata);
                    });
                }

                // Execute the modules once and apply to each input document
                foreach (IDocument result in context.Execute(_modules))
                {

                    foreach (KeyValuePair<string, object> kvp in result)
                    {
                        metadata[kvp.Key] = kvp.Value;
                    }
                }
                return inputs.Select(context, input => context.GetDocument(input,
                    _onlyIfNonExisting ? metadata.Where(x => !input.ContainsKey(x.Key)) : metadata));
            }

            return inputs.Select(context, x => _onlyIfNonExisting && x.ContainsKey(_key)
                ? x
                : context.GetDocument(x, new[] { new KeyValuePair<string, object>(_key, _metadata.GetValue(x, context)) }));
        }
    }
}
