using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Modules
{
    public class Meta : IModule
    {
        private readonly string _key;
        private readonly DocumentConfig _metadata;
        private readonly IModule[] _modules;
        private bool _forEachDocument;

        public Meta(string key, object metadata)
            : this(key, (x, y) => metadata)
        {
        }

        public Meta(string key, DocumentConfig metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _key = key;
            _metadata = metadata ?? ((x, y) => null);
        }

        // For performance reasons, the specified modules will only be run once with a newly initialized, isolated document
        // Otherwise, we'd need to run the whole set for each input document (I.e., multiple duplicate file reads, transformations, etc. for each input)
        // The metadata from all outputs will be added to the metadata for each input (possibly overwriting)
        public Meta(params IModule[] modules)
        {
            _modules = modules;
        }

        // Setting true for forEachDocument results in the whole sequence of modules being executed for every input document
        // (as opposed to only being executed once with an empty initial document)
        public Meta ForEachDocument()
        {
            _forEachDocument = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            if (_modules != null)
            {
                Dictionary<string, object> metadata = new Dictionary<string, object>();

                // Execute the modules for each input document
                if (_forEachDocument)
                {
                    return inputs.Select(input =>
                    {
                        foreach (IDocument result in context.Execute(_modules, new[] { input }))
                        {
                            foreach (KeyValuePair<string, object> kvp in result)
                            {
                                metadata[kvp.Key] = kvp.Value;
                            }
                        }
                        return input.Clone(metadata);
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
                return inputs.Select(input => input.Clone(metadata));
            }

            return inputs.Select(x => x.Clone(new [] { new KeyValuePair<string, object>(_key, _metadata(x, context)) }));
        }
    }
}
