using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpFixtureBase
    {
        protected IDocument GetResult(List<IDocument> results, string name)
        {
            return results.Single(x => x["Name"].Equals(name));
        }

        protected IDocument GetMember(List<IDocument> results, string className, string memberName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Single(x => x["Name"].Equals(memberName));
        }
    }
}
