using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Html;

namespace Wyam.Docs.Pipelines
{
    /// <summary>
    /// Loads blog posts from Markdown and/or Razor files.
    /// </summary>
    public class BlogPosts : Pipeline
    {
        internal BlogPosts(ConcurrentDictionary<string, string> typeNamesToLink)
            : base(GetModules(typeNamesToLink))
        {
        }

        private static IModuleList GetModules(ConcurrentDictionary<string, string> typeNamesToLink) => new ModuleList
        {
            new ReadFiles("blog/*.md"),
            new Meta(DocsKeys.EditFilePath, (doc, ctx) => doc.FilePath(Keys.RelativeFilePath)),
            new FrontMatter(new Yaml.Yaml()),
            new Execute(ctx => new Markdown.Markdown().UseExtensions(ctx.Settings.List<Type>(DocsKeys.MarkdownExternalExtensions)).UseConfiguration(ctx.String(DocsKeys.MarkdownExtensions))),
            new If(ctx => ctx.Bool(DocsKeys.AutoLinkTypes),
                new AutoLink(typeNamesToLink)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
            ),
            // This is an ugly hack to re-escape @ symbols in Markdown since AngleSharp unescapes them if it
            // changes text content to add an auto link, can be removed if AngleSharp #494 is addressed
            new If((doc, ctx) => doc.String(Keys.SourceFileExt) == ".md",
                new Replace("@", "&#64;")
            ),
            new Excerpt(),
            new Meta("FrontMatterPublished", (doc, ctx) => doc.ContainsKey(DocsKeys.Published)),  // Record whether the publish date came from front matter
            new Meta(DocsKeys.Published, (doc, ctx) =>
            {
                DateTime published;
                if (!ctx.TryParseInputDateTime(doc.String(Keys.SourceFileName).Substring(0, 10), out published))
                {
                    Wyam.Common.Tracing.Trace.Warning($"Could not parse published date for {doc.SourceString()}.");
                    return null;
                }
                return published;
            }).OnlyIfNonExisting(),
            new Where((doc, ctx) =>
            {
                if (!doc.ContainsKey(DocsKeys.Published) || doc.Get(DocsKeys.Published) == null)
                {
                    Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to not having {DocsKeys.Published} metadata");
                    return false;
                }
                if (doc.Get<DateTime>(DocsKeys.Published) > DateTime.Now)
                {
                    Common.Tracing.Trace.Warning($"Skipping {doc.SourceString()} due to having {DocsKeys.Published} metadata in the future of {doc.Get<DateTime>(DocsKeys.Published)} (current date and time is {DateTime.Now})");
                    return false;
                }
                return true;
            }),
            new Meta(Keys.RelativeFilePath, (doc, ctx) =>
            {
                DateTime published = doc.Get<DateTime>(DocsKeys.Published);
                string fileName = doc.Bool("FrontMatterPublished")
                    ? doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath
                    : doc.FilePath(Keys.SourceFileName).ChangeExtension("html").FullPath.Substring(11);
                return ctx.Bool(DocsKeys.IncludeDateInPostPath) ? $"blog/{published:yyyy}/{published:MM}/{fileName}" : $"blog/{fileName}";
            }),
            new OrderBy((doc, ctx) => doc.Get<DateTime>(DocsKeys.Published)).Descending()
        };
    }
}
