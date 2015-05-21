using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Extensibility;

namespace Wyam.Modules.Razor.Tests
{
    [TestFixture]
    public class RazorFixture
    {
        [Test]
        public void RazorTest()
        {
            // Given
            IPipelineContext context = Substitute.For<IPipelineContext>();
            context.RootFolder.Returns(Environment.CurrentDirectory);
            IDocument document = Substitute.For<IDocument>();
            IEnumerable<KeyValuePair<string, object>> items = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            document.Content.Returns(@"@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
            Razor razor = new Razor();

            // When
            razor.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
        }
    }
}
