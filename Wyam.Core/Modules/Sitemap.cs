using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;

namespace Wyam.Core.Modules
{
    public class Sitemap : IModule
    {
        private static readonly string[] ChangeFrequencies = { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        private readonly string _sitemapFilename;
        private readonly Func<string, string> _locationFormatter;
        private bool _shouldUseInputDocuments;

        public Sitemap(string sitemapFilename, Func<string, string> locationFormatter = null)
        {
            _sitemapFilename = sitemapFilename;
            _locationFormatter = locationFormatter;
        }

        public Sitemap FromInputDocuments()
        {
            _shouldUseInputDocuments = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            string outputFilename = Path.Combine(context.OutputFolder, _sitemapFilename);
            IDocument[] docs = (_shouldUseInputDocuments ? inputs.AsEnumerable() : context.Documents).Where(f => f.Metadata.ContainsKey(MetadataKeys.SitemapItem)).ToArray();

            if (docs.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

                foreach (IDocument doc in docs)
                {
                    object hostname;
                    doc.Metadata.TryGetValue(MetadataKeys.Hostname, out hostname);

                    object sitemapItemMetadata = doc.Metadata.Get(MetadataKeys.SitemapItem);
                    SitemapItem sitemapItem = sitemapItemMetadata as SitemapItem 
                        ?? new SitemapItem(sitemapItemMetadata.ToString());

                    if (string.IsNullOrWhiteSpace(sitemapItem.Location))
                    {
                        continue; // do not include this document for the sitemap
                    }

                    string location = PathHelper.ToLink(sitemapItem.Location);

                    if (_locationFormatter != null)
                    {
                        location = _locationFormatter(location);

                        if (string.IsNullOrWhiteSpace(location))
                        { 
                            continue; // location being null signals that this document should not be included in the sitemap
                        }
                    }
                    else
                    {
                        if (hostname != null && !location.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                        { 
                            location = $"{hostname.ToString().TrimEnd('/')}{PathHelper.ToRootLink(location)}";
                        }
                    }

                    sb.Append("<url>");
                    sb.AppendFormat("<loc>{0}</loc>", PathHelper.ToLink(location));

                    if (sitemapItem.LastModUtc.HasValue)
                    {
                        sb.AppendFormat("<lastmod>{0}</lastmod>", sitemapItem.LastModUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    }

                    if (sitemapItem.ChangeFrequency.HasValue)
                    { 
                        sb.AppendFormat("<changefreq>{0}</changefreq>", ChangeFrequencies[(int)sitemapItem.ChangeFrequency.Value]);
                    }

                    if (sitemapItem.Priority.HasValue)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "<priority>{0}</priority>", sitemapItem.Priority.Value);
                    }

                    sb.Append("</url>");
                }

                sb.Append("</urlset>");

                File.WriteAllText(outputFilename, sb.ToString());
            }

            return inputs;
        }
    }

    public class SitemapItem
    {
        public string Location { get; set; }
        public DateTime? LastModUtc { get; set; }
        public ChangeFrequency? ChangeFrequency { get; set; }
        public double? Priority { get; set; }

        public SitemapItem(string location)
        {
            Location = location;
        }
    }

    public enum ChangeFrequency
    {
        Always = 0,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Never
    }
}
