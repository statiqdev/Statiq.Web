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
        public void SimpleTemplateIsRendered()
        {
            // Given
            IPipelineContext context = Substitute.For<IPipelineContext>();
            context.RootFolder.Returns(Environment.CurrentDirectory);
            IDocument document = Substitute.For<IDocument>();
            document.Metadata.Get("FileBase", "/").Returns("/");
            List<string> items = new List<string>();
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => items.Add(x.Arg<string>()));
            document.Content.Returns(@"@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }");
            Razor razor = new Razor();

            // When
            razor.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(1, items.Count());
            Assert.AreEqual(" <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ", items.First());
        }
    }
}
