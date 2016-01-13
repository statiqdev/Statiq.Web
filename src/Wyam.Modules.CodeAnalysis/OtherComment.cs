using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.CodeAnalysis
{
    public class OtherComment
    {
        public IReadOnlyDictionary<string, string> Attributes { get; }
        public string Html { get; }

        internal OtherComment(IReadOnlyDictionary<string, string> attributes, string html)
        {
            Attributes = attributes;
            Html = html;
        }
    }
}
