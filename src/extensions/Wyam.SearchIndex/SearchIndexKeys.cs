using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.SearchIndex
{
    /// <summary>
    /// Metadata keys for use by the <see cref="SearchIndex"/> module.
    /// </summary>
    public static class SearchIndexKeys
    {
        /// <summary>
        /// Contains a <see cref="SearchIndexItem"/> that can be used to provide
        /// specific search index information for a given document.
        /// </summary>
        /// <type><see cref="SearchIndexItem"/></type>
        public const string SearchIndexItem = nameof(SearchIndexItem);
    }
}
