using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Sass.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SassFixture : BaseFixture
    {
        [Test]
        public void Convert()
        {
            string input = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

            string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

            IExecutionContext context = Substitute.For<IExecutionContext>();
            IDocument document = Substitute.For<IDocument>();
            document.Content.Returns(input);

            Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

            // When
            sass.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
            context.Received().GetDocument(document, output);
        }

        [Test]
        public void ConvertingBadSassFails()
        {
            string input = @"
$font-stack:    Helvetica, sans-serif
$primary-color: #333

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

            IExecutionContext context = Substitute.For<IExecutionContext>();
            IDocument document = Substitute.For<IDocument>();
            document.Content.Returns(input);

            Sass sass = new Sass();

            // That
            Assert.Catch<AggregateException>(() => 
            {
                sass.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list
            });
        }
    }
}
