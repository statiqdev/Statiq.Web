using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;
using Statiq.Handlebars;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;

namespace Statiq.Web
{
    public class Templates : IEnumerable<Template>, IReadOnlyDictionary<string, Template>
    {
        private readonly List<Template> _templates;

        private string _defaultTemplate;

        internal Templates()
        {
            // Create the default set of templates
            _templates = new List<Template>
            {
                new Template(Template.Markdown, Phase.Process, MediaTypes.Markdown, new RenderMarkdown().UseExtensions()),
                new Template(Template.Handlebars, MediaTypes.Handlebars, new RenderHandlebars()),
                new Template(
                    Template.Razor,
                    MediaTypes.Razor,
                    new RenderRazor()
                        .WithLayout(Config.FromDocument((doc, ctx) =>
                        {
                            // Crawl up the tree looking for a layout
                            DocumentPathTree<IDocument> tree = ctx.Outputs.AsSourceTree();
                            IDocument parent = doc;
                            while (parent is object)
                            {
                                if (parent.ContainsKey(WebKeys.Layout))
                                {
                                    return parent.GetPath(WebKeys.Layout);
                                }
                                parent = tree.GetParentOf(parent);
                            }
                            return null;  // If no layout metadata, revert to default behavior
                        }))),
            };

            _defaultTemplate = Template.Razor;
        }

        /// <summary>
        /// The default template to render. It should already be added to the
        /// collection and be set to execute during the <see cref="Phase.PostProcess"/>
        /// phase. The default template will always be executed last for all documents,
        /// regardless of the specified condition. Use <c>null</c> to indicate no default template.
        /// </summary>
        public string DefaultTemplate
        {
            get => _defaultTemplate;
            set
            {
                if (value is object)
                {
                    // If not null, do some sanity checks
                    if (!TryGetValue(value, out Template template))
                    {
                        throw new ArgumentException($"The template {value} does not exist");
                    }
                    if (template.Phase != Phase.PostProcess)
                    {
                        throw new ArgumentException("The default template must execute during the post-process phase");
                    }
                }
                _defaultTemplate = value;
            }
        }

        internal IEnumerable<IModule> GetModules(Phase phase)
        {
            IEnumerable<IModule> modules = _templates
                .Where(x => x.Phase == phase && (phase != Phase.PostProcess || !x.Name.Equals(DefaultTemplate, StringComparison.OrdinalIgnoreCase)))
                .Select(x => new ExecuteIf(x.Condition, x.Module));
            if (phase == Phase.PostProcess && DefaultTemplate is object)
            {
                if (!TryGetValue(DefaultTemplate, out Template defaultTemplate))
                {
                    // Sanity check, should never get here
                    throw new ExecutionException("Could not find the default template");
                }
                modules = modules.Concat(defaultTemplate.Module);
            }
            return modules;
        }

        public Template this[int index] => _templates[index];

        public int Count => _templates.Count;

        public void Add(Template template)
        {
            template.ThrowIfNull(nameof(template));

            if (Contains(template.Name))
            {
                throw new ArgumentException($"A template with the name {template.Name} has already been added");
            }

            _templates.Add(template);
        }

        public int IndexOf(string name)
        {
            name.ThrowIfNull(nameof(name));

            return _templates.FindIndex(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void Insert(int index, Template template)
        {
            template.ThrowIfNull(nameof(template));

            if (Contains(template.Name))
            {
                throw new ArgumentException($"A template with the name {template.Name} has already been added");
            }

            _templates.Insert(index, template);
        }

        public bool Remove(string name)
        {
            name.ThrowIfNull(nameof(name));

            int index = IndexOf(name);
            if (index < 0)
            {
                return false;
            }
            if (_templates[index].Name.Equals(_defaultTemplate, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Cannot remove the default template (set the default template to something else first)");
            }
            _templates.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes all templates. This will also set the <see cref="DefaultTemplate"/> to <c>null</c>.
        /// </summary>
        public void Clear()
        {
            _defaultTemplate = null;
            _templates.Clear();
        }

        public bool Contains(string name)
        {
            name.ThrowIfNull(nameof(name));

            return _templates.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerator<Template> GetEnumerator() => ((IList<Template>)_templates).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IList<Template>)_templates).GetEnumerator();

        // IReadOnlyDictionary<Template>

        public Template this[string name]
        {
            get
            {
                name.ThrowIfNull(nameof(name));

                Template template = _templates.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (template == default)
                {
                    throw new KeyNotFoundException();
                }
                return template;
            }
        }

        public bool TryGetValue(string name, out Template template)
        {
            name.ThrowIfNull(nameof(name));

            template = _templates.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return template != default;
        }

        IEnumerable<string> IReadOnlyDictionary<string, Template>.Keys => _templates.Select(x => x.Name);

        IEnumerable<Template> IReadOnlyDictionary<string, Template>.Values => _templates;

        int IReadOnlyCollection<KeyValuePair<string, Template>>.Count => throw new NotImplementedException();

        Template IReadOnlyDictionary<string, Template>.this[string key] => throw new NotImplementedException();

        bool IReadOnlyDictionary<string, Template>.ContainsKey(string name) => Contains(name);

        IEnumerator<KeyValuePair<string, Template>> IEnumerable<KeyValuePair<string, Template>>.GetEnumerator() =>
            _templates.Select(x => new KeyValuePair<string, Template>(x.Name, x)).GetEnumerator();
    }
}
