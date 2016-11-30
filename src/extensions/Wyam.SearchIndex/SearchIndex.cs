using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.SearchIndex
{
    /// <summary>
    /// Generates a JavaScript-based search index from the input documents.
    /// </summary>
    /// <remarks>
    /// This module generates a search index that can be imported into the JavaScript <a href="http://lunrjs.com/">Lunr.js</a> search engine.
    /// Each input document should either specify the <c>SearchIndexItem</c> metadata key or a delegate that returns a <c>SearchIndexItem</c>
    /// instance.
    /// </remarks>
    /// <example>
    /// The client-side JavaScript code for importing the search index should look something like this (assuming you have an HTML <c>input</c>
    /// with an ID of <c>#search</c> and a <c>div</c> with an ID of <c>#search-results</c>):
    /// <code>
    /// function runSearch(query) {
	///     $("#search-results").empty();
    ///     if (query.length &lt; 2)
    ///     {
    ///         return;
    ///     }
    ///     var results = searchModule.search(query);
    ///     var listHtml = "&lt;ul&gt;";
    ///     listHtml += "&lt;li&gt;&lt;strong&gt;Search Results&lt;/strong&gt;&lt;/li&gt;";
    ///     if (results.length == 0)
    ///     {
    ///         listHtml += "&lt;li&gt;No results found&lt;/li&gt;";
    ///     }
    ///     else
    ///     {
    ///         for (var i = 0; i &lt; results.length; ++i)
    ///         {
    ///             var res = results[i];
    ///             listHtml += "&lt;li&gt;&lt;a href='" + res.url + "'&gt;" + res.title + "&lt;/a&gt;&lt;/li&gt;";
    ///         }
    ///     }
    ///     listHtml += "&lt;/ul&gt;";				
    ///     $("#search-results").append(listHtml);
    /// }
    /// 
    /// $(document).ready(function() {
    ///     $("#search").on('input propertychange paste', function() {
    ///         runSearch($("#search").val());
    ///     });
    /// });
    /// </code>
    /// </example>
    /// <category>Content</category>
    public class SearchIndex : IModule
    {
        private static readonly Regex StripHtmlAndSpecialChars = new Regex(@"<[^>]+>|&[a-z]{2,};|&#\d+;|[^a-z-#]", RegexOptions.Compiled);
        private readonly DocumentConfig _searchIndexItem;
        private readonly FilePath _stopwordsPath;
        private readonly bool _enableStemming;
        private bool _includeHost = true;

        /// <summary>
        /// Creates the search index by looking for a <c>SearchIndexItem</c> metadata key in each input document that 
        /// contains a <c>SearchIndexItem</c> instance.
        /// </summary>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public SearchIndex(FilePath stopwordsPath = null, bool enableStemming = false)
            : this((doc, ctx) => doc.Get<SearchIndexItem>(SearchIndexKeys.SearchIndexItem), stopwordsPath, enableStemming)
        {
        }

        /// <summary>
        /// Creates the search index by looking for a specified metadata key in each input document that 
        /// contains a <c>SearchIndexItem</c> instance.
        /// </summary>
        /// <param name="searchIndexItemMetadataKey">The metadata key that contains the <c>SearchIndexItem</c> instance.</param>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        public SearchIndex(string searchIndexItemMetadataKey, FilePath stopwordsPath = null, bool enableStemming = false)
            : this((doc, ctx) => doc.Get<SearchIndexItem>(searchIndexItemMetadataKey), stopwordsPath, enableStemming)
        {
        }

        /// <summary>
        /// Creates the search index by using a delegate that returns a <c>SearchIndexItem</c> instance for each input document.
        /// </summary>
        /// <param name="searchIndexItem">A delegate that should return a <c>SearchIndexItem</c>.</param>
        /// <param name="stopwordsPath">A file to use that contains a set of stopwords.</param>
        /// <param name="enableStemming">If set to <c>true</c>, stemming is enabled.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public SearchIndex(DocumentConfig searchIndexItem, FilePath stopwordsPath = null, bool enableStemming = false)
        {
            if (searchIndexItem == null)
            {
                throw new ArgumentNullException(nameof(searchIndexItem));
            }
            _searchIndexItem = searchIndexItem;
            _stopwordsPath = stopwordsPath;
            _enableStemming = enableStemming;
        }

        /// <summary>
        /// Indicates whether the host should be automatically included in generated links.
        /// </summary>
        /// <param name="includeHost"><c>true</c> to include the host.</param>
        public SearchIndex IncludeHost(bool includeHost = true)
        {
            _includeHost = includeHost;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            SearchIndexItem[] searchIndexItems = inputs
                .Select(x => _searchIndexItem.TryInvoke<SearchIndexItem>(x, context))
                .Where(x => x != null 
                    && !string.IsNullOrEmpty(x.Url) 
                    && !string.IsNullOrEmpty(x.Title) 
                    && !string.IsNullOrEmpty(x.Content))
                .ToArray();

            if( searchIndexItems.Length == 0 )
            {
                Trace.Warning("It's not possible to build the search index because no documents contain the necessary metadata.");
                return Array.Empty<IDocument>();
            }
            
            string[] stopwords = GetStopwords(context);
            string jsFileContent = BuildSearchIndex(searchIndexItems, stopwords, context);
            return new []{ context.GetDocument(jsFileContent) };
        }
        
        private string BuildSearchIndex(IList<SearchIndexItem> searchIndexItems, string[] stopwords, IExecutionContext context)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < searchIndexItems.Count(); ++i)
            {
                SearchIndexItem itm = searchIndexItems.ElementAt(i);
                sb.AppendLine($@"a({{
id:{i},
title:{CleanString(itm.Title, stopwords)},
content:{CleanString(itm.Content, stopwords)},
description:{CleanString(itm.Description, stopwords)},
tags:'{itm.Tags}'
}});");
            }

            foreach (SearchIndexItem itm in searchIndexItems)
            {
                sb.AppendLine($@"y({{
url:'{context.GetLink(new FilePath(itm.Url), _includeHost)}',
title:{ToJsonString(itm.Title)},
description:{ToJsonString(itm.Description)}
}});");
            }

            return CreateJs(sb.ToString());
        }

        private string CreateJs(string dynamicJsContent)
        {
            return @"var searchModule = function() {
var idMap = [];
function y(e){idMap.push(e);}
var idx = lunr(function() {
this.field('title', { boost: 10})
this.field('content')
this.field('description', { boost: 5})
this.field('tags', { boost: 50})
this.ref('id')

this.pipeline.remove(lunr.stopWordFilter);" + (_enableStemming ? "" : "this.pipeline.remove(lunr.stemmer);") + @"
})
function a(e){idx.add(e);}

" + dynamicJsContent + @"
return {
search: function(q) {return idx.search(q).map(function(i){return idMap[i.ref];});}
};
}();";
        }

        private static string CleanString(string input, string[] stopwords)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "''";
            }

            string clean = input.ToLowerInvariant();
            clean = StripHtmlAndSpecialChars.Replace(clean, " ").Trim();
            clean = Regex.Replace(clean, @"\s{2,}", " ");
            clean = string.Join(" ", clean.Split(' ').Where(f => f.Length > 1 && !stopwords.Contains(f)).ToArray());
            clean = ToJsonString(clean);

            return clean;
        }

        private static string ToJsonString(string s)
        {
            return Newtonsoft.Json.JsonConvert.ToString(s);
        }

        private string[] GetStopwords(IExecutionContext context)
        {
            string[] stopwords = new string[0];

            if (_stopwordsPath != null)
            {
                IFile stopwordsFile = context.FileSystem.GetInputFile(_stopwordsPath);
                if (stopwordsFile.Exists)
                {
                    stopwords = stopwordsFile.ReadAllText()
                        .Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim().ToLowerInvariant())
                        .Where(f => f.Length > 1)
                        .ToArray();
                }
            }

            return stopwords;
        }
    }
}
