using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpFixtureBase
    {
        protected IDocument GetClass(List<IDocument> results, string className)
        {
            return results.Single(x => x["Name"].Equals(className));
        }

        protected IDocument GetMember(List<IDocument> results, string className, string memberName)
        {
            return GetClass(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Single(x => x["Name"].Equals(memberName));
        }
    }
}
