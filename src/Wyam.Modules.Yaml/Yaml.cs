using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using YamlDotNet.Dynamic;
using YamlDotNet.RepresentationModel;

namespace Wyam.Modules.Yaml
{
    /// <summary>
    /// Parses YAML content for each input document and stores the result in it's metadata.
    /// </summary>
    /// <remarks>
    /// Parses the content for each input document and then stores a dynamic object 
    /// representing the first YAML document in metadata with the specified key. If no key is specified, 
    /// then the dynamic object is not added. You can also flatten the YAML to add top-level pairs directly
    /// to the document metadata.
    /// </remarks>
    /// <category>Metadata</category>
    public class Yaml : IModule
    {
        private readonly bool _flatten;
        private readonly string _key;

        /// <summary>
        /// The content of the input document is parsed as YAML. All root-level scalars are added to the input document's 
        /// metadata. Any more complex YAML structures are ignored. This is best for simple key-value YAML documents.
        /// </summary>
        public Yaml()
        {
            _flatten = true;
        }

        /// <summary>
        /// The content of the input document is parsed as YAML. A dynamic object representing the first YAML 
        /// document is set as the value for the given metadata key. See YamlDotNet.Dynamic for more details 
        /// about the dynamic YAML object. If flatten is true, all root-level scalars are also added 
        /// to the input document's metadata.
        /// </summary>
        /// <param name="key">The metadata key in which to set the dynamic YAML object.</param>
        /// <param name="flatten">If set to <c>true</c>, all root-level scalars are also added to the input document's metadata.</param>
        public Yaml(string key, bool flatten = false)
        {
            _key = key;
            _flatten = flatten;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(input =>
                {
                    try
                    {
                        Dictionary<string, object> items = new Dictionary<string, object>();
                        using (TextReader contentReader = new StringReader(input.Content))
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
                                        // Map scalar-to-scalar children
                                        foreach (KeyValuePair<YamlNode, YamlNode> child in 
                                            ((YamlMappingNode)document.RootNode).Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlScalarNode))
                                        {
                                            items[((YamlScalarNode)child.Key).Value] = ((YamlScalarNode)child.Value).Value;
                                        }

                                        // Map simple sequences
                                        foreach (KeyValuePair<YamlNode, YamlNode> child in
                                            ((YamlMappingNode) document.RootNode).Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlSequenceNode && ((YamlSequenceNode)y.Value).All(z => z is YamlScalarNode)))
                                        {
                                            items[((YamlScalarNode)child.Key).Value] = ((YamlSequenceNode)child.Value).Select(a => ((YamlScalarNode)a).Value).ToArray();
                                        }
                                    }
                                }
                            }
                        }
                        return context.GetDocument(input, items);
                    }
                    catch (Exception ex)
                    {
                        Trace.Error("Error processing YAML for {0}: {1}", input.SourceString(), ex.ToString());
                    }
                    return null;
                })
                .Where(x => x != null);
        }
    }
}
