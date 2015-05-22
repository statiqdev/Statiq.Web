using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;
using YamlDotNet.Dynamic;
using YamlDotNet.RepresentationModel;

namespace Wyam.Modules.Yaml
{
    // Parses the content for each input document and then stores a dynamic object representing the first YAML document in metadata with the specified key
    // If no key is specified, then the dynamic object is not added
    // Flatten indicates that top-level pairs should be added to the metadata root 
    public class Yaml : IModule
    {
        private readonly bool _flatten;
        private readonly string _key;

        public Yaml(bool flatten = true)
        {
            _flatten = flatten;
        }

        public Yaml(string key, bool flatten = false)
        {
            _key = key;
            _flatten = flatten;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x =>
            {
                Dictionary<string, object> items = new Dictionary<string, object>();
                using (TextReader contentReader = new StringReader(x.Content))
                {
                    YamlStream yamlStream = new YamlStream();
                    yamlStream.Load(contentReader);
                    if (yamlStream.Documents.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(_key))
                        {
                            items[_key] = new DynamicYaml(yamlStream.Documents[0].RootNode);
                        }
                        if (_flatten)
                        {
                            foreach (YamlDocument document in yamlStream.Documents)
                            {
                                foreach (KeyValuePair<YamlNode, YamlNode> child in 
                                    ((YamlMappingNode)document.RootNode).Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlScalarNode))
                                {
                                    items[((YamlScalarNode)child.Key).Value] = ((YamlScalarNode)child.Value).Value;
                                }
                            }
                        }
                    }
                }
                return x.Clone(items);
            });
        }
    }
}
