using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpBaseFixture : BaseFixture
    {
        protected IDocument GetDocument(string content) => new TestDocument(content);

        protected IExecutionContext GetContext() => new TestExecutionContext
        {
            FileSystem = new TestFileSystem
            {
                RootPath = new DirectoryPath(TestContext.CurrentContext.TestDirectory)
            }
        };

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

        protected IDocument GetParameter(List<IDocument> results, string className, string methodName, string parameterName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Single(x => x["Name"].Equals(methodName))
                .Get<IEnumerable<IDocument>>("Parameters")
                .Single(x => x["Name"].Equals(parameterName));
        }
    }
}
