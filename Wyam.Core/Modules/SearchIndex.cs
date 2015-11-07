using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules
{
    public class SearchIndex : IModule
    {
        private string searchIndexOutputFilename;

        public SearchIndex(string searchIndexOutputFilename)
        {
            this.searchIndexOutputFilename = searchIndexOutputFilename;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var searchIndexItems = context.Documents.Where(f => f.ContainsKey("SearchIndexItem")).Select(f => f["SearchIndexItem"]).OfType<SearchIndexItem>().ToArray();

            if( searchIndexItems.Length == 0 )
            {
                context.Trace.Warning("It's not possible to build the search index, because no documents contain the meta data 'SearchIndexItem'.");
                return inputs;
            }

            var indexedItems = new StringBuilder();

            foreach (var itm in searchIndexItems)
            {
                indexedItems.AppendLine(AddToIndexJs(itm));
            }

            var jsFileContent = string.Format(ModuleJs, indexedItems.ToString());
            File.WriteAllText(this.searchIndexOutputFilename, jsFileContent);

            return inputs;
        }

        private string AddToIndexJs(SearchIndexItem itm)
        {
            return $@"idx.add({{
	url: '{itm.Url}',
    title: '{CleanString(itm.Title)}',
    content: '{CleanString(itm.Content)}',
	description: '{CleanString(itm.Description)}',
    tags: '{itm.Tags}'
  }});";
        }

        private string CleanString(string input)
        {
            return input?.Replace("'", "\\'");
        }
        
        private const string ModuleJs = @"var searchModule = function() {
	var idx = lunr(function () {
    this.field('title', {boost: 10})
    this.field('content')
	this.field('description', {boost: 5})
	this.field('tags', {boost: 100})
    this.ref('url')
  })
  
  {0}
  
  return {
	search: function(query) {
		return idx.search(query)
	}
  };
}";
    }

    public class SearchIndexItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }

        public SearchIndexItem(string url, string title, string content)
        {
            this.Url = url;
            this.Title = title;
            this.Content = content;
        }
    }
}
