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
using Statiq.Less;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Sass;
using Statiq.Yaml;

namespace Statiq.Web
{
    public class Templates : IReadOnlyList<Template>, IReadOnlyDictionary<string, Template>
    {
        private readonly List<KeyValuePair<string, Template>> _templates = new List<KeyValuePair<string, Template>>();

        internal Templates()
        {
            // Assets
            Add(MediaTypes.Less, new Template(ContentType.Asset, Phase.Process, new CacheDocuments { new CompileLess() }));
            Add(MediaTypes.Sass, new Template(ContentType.Asset, Phase.Process, new CacheDocuments { new CompileSass().WithCompactOutputStyle() }));
            Add(MediaTypes.Scss, this[MediaTypes.Sass]);

            // Data
            Add(MediaTypes.Json, new Template(ContentType.Data, Phase.Process, new ParseJson()));
            Add(MediaTypes.Yaml, new Template(ContentType.Data, Phase.Process, new ParseYaml()));

            // Content (Process)
            Add(MediaTypes.Markdown, new Template(ContentType.Content, Phase.Process, new RenderMarkdown().UseExtensions()));

            // Content (PostProcess)
            Add(MediaTypes.Handlebars, new Template(ContentType.Content, Phase.PostProcess, new RenderHandlebars()));
            Add(
                MediaTypes.Razor,
                new Template(
                    ContentType.Content,
                    Phase.PostProcess,
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
                        }))));

            // Set Razor as the default for HTML and HTML fragment files, but if HTML make sure we don't already have a full <html> before applying layouts
            // By changing the module of MediaTypes.HtmlFragment we can change the default layout template
            Add(MediaTypes.HtmlFragment, this[MediaTypes.Razor]);
            Add(
                MediaTypes.Html,
                new Template(
                    ContentType.Content,
                    Phase.PostProcess,
                    new ExecuteIf(Config.FromDocument(async doc => !(await doc.GetContentStringAsync()).Replace(" ", string.Empty).Contains("<html")))
                    {
                        new ExecuteConfig(Config.FromContext(_ => this[MediaTypes.HtmlFragment].Module))
                    }));
        }

        /// <summary>
        /// Gets a single module that runs all the matching templates for a given set of input documents.
        /// </summary>
        /// <param name="contentType">The content type being processed.</param>
        /// <param name="phase">The phase being executed.</param>
        /// <returns>A module.</returns>
        public IModule GetModule(ContentType contentType, Phase phase)
        {
            ExecuteIf module = null;
            foreach (KeyValuePair<string, Template> item in _templates.Where(x => x.Value.ContentType == contentType && x.Value.Phase == phase))
            {
                if (item.Value.Module is object)
                {
                    // Add else conditions instead of lots of individual ExecuteIf modules so that we only run a single template
                    module = module is null
                        ? new ExecuteIf(Config.FromDocument(doc => doc.MediaTypeEquals(item.Key)), item.Value.Module)
                        : module.ElseIf(Config.FromDocument(doc => doc.MediaTypeEquals(item.Key)), item.Value.Module);
                }
            }
            return module;
        }

        public IEnumerable<string> GetMediaTypes(ContentType contentType) =>
            _templates.Where(x => x.Value.ContentType == contentType).Select(x => x.Key);

        public IEnumerable<string> GetMediaTypes(ContentType contentType, Phase phase) =>
            _templates.Where(x => x.Value.ContentType == contentType && x.Value.Phase == phase).Select(x => x.Key);

        public void Add(string mediaType, Template template)
        {
            template.ThrowIfNull(nameof(template));

            if (ContainsKey(mediaType))
            {
                throw new ArgumentException($"A template for media type {mediaType} has already been added");
            }

            _templates.Add(new KeyValuePair<string, Template>(mediaType, template));
        }

        public void Insert(int index, string mediaType, Template template)
        {
            template.ThrowIfNull(nameof(template));

            if (ContainsKey(mediaType))
            {
                throw new ArgumentException($"A template for media type {mediaType} has already been added");
            }

            _templates.Insert(index, new KeyValuePair<string, Template>(mediaType, template));
        }

        public bool Remove(string key)
        {
            int index = IndexOf(key);
            if (index < 0)
            {
                return false;
            }
            _templates.RemoveAt(index);
            return true;
        }

        public int IndexOf(string key) =>
            _templates.FindIndex(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        public void Clear() => _templates.Clear();

        // Interfaces

        public Template this[int index] => _templates[index].Value;

        public Template this[string key] => _templates.Find(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;

        public int Count => _templates.Count;

        public IEnumerable<string> Keys => _templates.Select(x => x.Key);

        public IEnumerable<Template> Values => _templates.Select(x => x.Value);

        public bool ContainsKey(string key) => _templates.Any(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Template value)
        {
            int index = IndexOf(key);
            if (index < 0)
            {
                value = default;
                return false;
            }
            value = _templates[index].Value;
            return true;
        }

        public IEnumerator<Template> GetEnumerator() => _templates.Select(x => x.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<string, Template>> IEnumerable<KeyValuePair<string, Template>>.GetEnumerator() => _templates.GetEnumerator();
    }
}
