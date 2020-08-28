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

        internal Templates()
        {
            // Create the default set of templates
            _templates = new List<Template>
            {
                // ContentProcess
                new Template(nameof(MediaTypes.Markdown), TemplateType.ContentProcess, MediaTypes.Markdown, new RenderMarkdown().UseExtensions()),

                // ContentPostProcess
                new Template(nameof(MediaTypes.Handlebars), TemplateType.ContentPostProcess, MediaTypes.Handlebars, new RenderHandlebars()),

                // Razor is last and processes both Razor and HTML media types so it runs for other templates and HTML files
                // Change the condition for Razor to just the Razor media type to prevent it from always running
                new Template(
                    nameof(MediaTypes.Razor),
                    TemplateType.ContentPostProcess,
                    new[] { MediaTypes.Razor, MediaTypes.Html },
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
        }

        internal IEnumerable<IModule> GetModules(TemplateType templateType) =>
            _templates
                .Where(x => x.TemplateType == templateType)
                .Select(x => new ExecuteIf(x.Condition, x.Module));

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
            _templates.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes all templates.
        /// </summary>
        public void Clear() => _templates.Clear();

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
