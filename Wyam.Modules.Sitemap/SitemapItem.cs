using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.Sitemap
{
    public class SitemapItem
    {
        public string Location { get; set; }
        public DateTime? LastModUtc { get; set; }
        public ChangeFrequency? ChangeFrequency { get; set; }
        public double? Priority { get; set; }

        public SitemapItem(string location)
        {
            this.Location = location.Replace('\\', '/');
        }
    }
}
