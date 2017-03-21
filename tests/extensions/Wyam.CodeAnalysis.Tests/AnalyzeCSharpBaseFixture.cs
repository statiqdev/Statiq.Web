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
using Wyam.Testing;
using Wyam.Testing.Documents;

namespace Wyam.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpBaseFixture : BaseFixture
    {
        protected IDocument GetDocument(string content)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            return document;
        }

        protected IExecutionContext GetContext()
        {
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.FileSystem.RootPath.Returns(new DirectoryPath(TestContext.CurrentContext.TestDirectory));
            context.GetLink(Arg.Any<NormalizedPath>(), Arg.Any<bool>()).Returns("link");
            context.GetDocument(Arg.Any<FilePath>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            return context;
        }

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
