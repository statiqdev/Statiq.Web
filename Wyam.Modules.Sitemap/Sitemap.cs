using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Sitemap
{
    public class Sitemap : IModule
    {
        private string sitemapFilename;
        private Func<string, string> locationFormatter;
        private static readonly string[] ChangeFrequencies = new string[] { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        public Sitemap(string sitemapFilename, Func<string, string> locationFormatter = null)
        {
            this.sitemapFilename = sitemapFilename;

            if (locationFormatter == null)
                locationFormatter = f => f;

            this.locationFormatter = locationFormatter;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var outputFilename = Path.Combine(context.OutputFolder, this.sitemapFilename);
            var docs = context.Documents.Where(f => f.Metadata.ContainsKey(MetadataKeys.SitemapItem)).ToArray();

            if (docs.Length > 0)
            {
                var sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

                foreach (var doc in docs)
                {
                    var itm = (SitemapItem)doc.Metadata.Get(MetadataKeys.SitemapItem);

                    sb.Append("<url>");
                    sb.AppendFormat("<loc>{0}</loc>", this.locationFormatter(itm.Location));

                    if (itm.LastModUtc.HasValue)
                        sb.AppendFormat("<lastmod>{0}</lastmod>", itm.LastModUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                    if (itm.ChangeFrequency.HasValue)
                        sb.AppendFormat("<changefreq>{0}</changefreq>", ChangeFrequencies[(int)itm.ChangeFrequency.Value]);

                    if (itm.Priority.HasValue)
                        sb.AppendFormat(CultureInfo.InvariantCulture, "<priority>{0}</priority>", itm.Priority.Value);

                    sb.Append("</url>");
                }

                sb.Append("</urlset>");

                File.WriteAllText(outputFilename, sb.ToString());
            }

            return inputs;
        }
    }
}
