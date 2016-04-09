using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Modules.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class CssMinifyTests : BaseFixture
    {
        public class ExecuteMethodTests : CssMinifyTests
        {
            [Test]
            public void Minify()
            {
                // Given
                // Example taken from http://yui.github.io/yuicompressor/css.html
                string input = @"
/*****
  Multi-line comment
  before a new class name
*****/
.classname {
    /* comment in declaration block */
    font-weight: normal;
}";
                string output = @".classname{font-weight:normal}";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                CssMinify cssMinify = new CssMinify();

                // When
                cssMinify.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}