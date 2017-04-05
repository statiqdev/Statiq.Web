using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Represents a custom XML documentation comment element.
    /// </summary>
    public class OtherComment
    {
        /// <summary>
        /// The XML attributes the custom documentation comment element contains.
        /// </summary>
        public IReadOnlyDictionary<string, string> Attributes { get; }

        /// <summary>
        /// The rendered HTML of the custom documentation comment element content.
        /// </summary>
        public string Html { get; }

        internal OtherComment(IReadOnlyDictionary<string, string> attributes, string html)
        {
            Attributes = attributes;
            Html = html;
        }
    }
}
