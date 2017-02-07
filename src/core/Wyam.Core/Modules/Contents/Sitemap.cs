using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Generates a sitemap from the input documents.
    /// </summary>
    /// <remarks>
    /// This module generates a sitemap from the input documents. The output document contains the sitemap XML as it's content.
    /// You can supply a location for the each item in the sitemap as a <c>string</c> (with an optional function to format it
    /// into an absolute HTML path) or you can supply a <c>SitemapItem</c> for more control. You can also specify the
    /// <c>Hostname</c> metadata key (as a <c>string</c>) for each input document, which will be prepended to all locations.
    /// </remarks>
    /// <category>Content</category>
    public class Sitemap : IModule
    {
        private static readonly string[] ChangeFrequencies = { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        private readonly DocumentConfig _sitemapItemOrLocation;
        private readonly Func<string, string> _locationFormatter;

        /// <summary>
        /// Creates a sitemap using the metadata key <c>SitemapItem</c> which should contain either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information.
        /// </summary>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the <c>SitemapItem</c> metadata key.</param>
        public Sitemap(Func<string, string> locationFormatter = null)
            : this((d, c) => d.Get(Keys.SitemapItem), locationFormatter)
        {
        }

        /// <summary>
        /// Creates a sitemap using the specified metadata key which should contain either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information.
        /// </summary>
        /// <param name="sitemapItemOrLocationMetadataKey">A metadata key that contains either a <c>SitemapItem</c> or 
        /// a <c>string</c> location for each input document.</param>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the specified metadata key.</param>
        public Sitemap(string sitemapItemOrLocationMetadataKey, Func<string, string> locationFormatter = null)
            : this((d, c) => d.String(sitemapItemOrLocationMetadataKey), locationFormatter)
        {
            if (String.IsNullOrEmpty(sitemapItemOrLocationMetadataKey))
            {
                throw new ArgumentException("Argument is null or empty", nameof(sitemapItemOrLocationMetadataKey));
            }
        }

        /// <summary>
        /// Creates a sitemap using the specified delegate which should return either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information.
        /// </summary>
        /// <param name="sitemapItemOrLocation">A delegate that either returns a <c>SitemapItem</c> instance or a <c>string</c> 
        /// with the desired item location. If the delegate returns <c>null</c>, the input document is not added to the sitemap.</param>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the specified metadata key.</param>
        public Sitemap(DocumentConfig sitemapItemOrLocation, Func<string, string> locationFormatter = null)
        {
            if (sitemapItemOrLocation == null)
            {
                throw new ArgumentNullException(nameof(sitemapItemOrLocation));
            }
            _sitemapItemOrLocation = sitemapItemOrLocation;
            _locationFormatter = locationFormatter;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            context.ForEach(inputs, input =>
            {
                // Try to get a SitemapItem
                object delegateResult = _sitemapItemOrLocation(input, context);
                SitemapItem sitemapItem = delegateResult as SitemapItem;
                if (sitemapItem == null)
                {
                    string locationDelegateResult = delegateResult as string;
                    if (!string.IsNullOrWhiteSpace(locationDelegateResult))
                    {
                        sitemapItem = new SitemapItem(locationDelegateResult);
                    }
                }

                // Add a sitemap entry if we got an item and valid location
                if (!string.IsNullOrWhiteSpace(sitemapItem?.Location))
                {
                    string location = sitemapItem.Location;

                    // Apply the location formatter if there is one
                    if (_locationFormatter != null)
                    {
                        location = _locationFormatter(location);
                    }

                    // Apply the hostname if defined (and the location formatter didn't already set a hostname)
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        if (!location.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                            && !location.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                        {
                            location = context.GetLink(new FilePath(location), true);
                        }
                    }

                    // Location being null signals that this document should not be included in the sitemap
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        sb.Append("<url>");
                        sb.AppendFormat("<loc>{0}</loc>", location);

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
                }
            });

            // Always output the sitemap document, even if it's empty
            sb.Append("</urlset>");
            return new[] { context.GetDocument(sb.ToString()) };
        }
    }
}
