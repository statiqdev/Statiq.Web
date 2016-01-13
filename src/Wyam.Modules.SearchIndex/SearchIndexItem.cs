using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.SearchIndex
{
    public class SearchIndexItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }

        public SearchIndexItem(string url, string title, string content)
        {
            Url = url;
            Title = title;
            Content = content;
        }
    }
}
