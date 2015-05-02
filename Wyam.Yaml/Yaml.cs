using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;
using YamlDotNet.Dynamic;

namespace Wyam.Yaml
{
    // Parses the content for each input document and then stores a dynamic object representing the YAML in metadata with the specified key
    public class Yaml : IModule
    {
        private readonly string _key;

        public Yaml(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            _key = key;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x => x.Clone(new[] { new KeyValuePair<string, object>(_key, new DynamicYaml(x.Content)) }));
        }
    }
}
