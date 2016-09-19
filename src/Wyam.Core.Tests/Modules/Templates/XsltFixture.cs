using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Templates;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Templates
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class XsltFixture : BaseFixture
    {
        public class ExecuteTests : XsltFixture
        {
            [Test]
            public void TestTransform()
            {
                // Given
                string xsltInput = ""
    + "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">"
    + "<xsl:template match=\"bookstore\">"
    + "  <HTML>"
    + "    <BODY>"
    + "      <TABLE BORDER=\"2\">"
    + "        <TR>"
    + "          <TD>ISBN</TD>"
    + "          <TD>Title</TD>"
    + "          <TD>Price</TD>"
    + "        </TR>"
    + "        <xsl:apply-templates select=\"book\"/>"
    + "      </TABLE>"
    + "    </BODY>"
    + "  </HTML>"
    + "</xsl:template>"
    + "<xsl:template match=\"book\">"
    + "  <TR>"
    + "    <TD><xsl:value-of select=\"@ISBN\"/></TD>"
    + "    <TD><xsl:value-of select=\"title\"/></TD>"
    + "    <TD><xsl:value-of select=\"price\"/></TD>"
    + "  </TR>"
    + "</xsl:template>"
    + "</xsl:stylesheet>";

                string input = ""
    + "<?xml version='1.0'?>"
    + "<!-- This file represents a fragment of a book store inventory database -->"
    + "<bookstore>"
    + "  <book genre=\"autobiography\" publicationdate=\"1981\" ISBN=\"1-861003-11-0\">"
    + "    <title>The Autobiography of Benjamin Franklin</title>"
    + "    <author>"
    + "      <first-name>Benjamin</first-name>"
    + "      <last-name>Franklin</last-name>"
    + "    </author>"
    + "    <price>8.99</price>"
    + "  </book>"
    + "  <book genre=\"novel\" publicationdate=\"1967\" ISBN=\"0-201-63361-2\">"
    + "    <title>The Confidence Man</title>"
    + "    <author>"
    + "      <first-name>Herman</first-name>"
    + "      <last-name>Melville</last-name>"
    + "    </author>"
    + "    <price>11.99</price>"
    + "  </book>"
    + "  <book genre=\"philosophy\" publicationdate=\"1991\" ISBN=\"1-861001-57-6\">"
    + "    <title>The Gorgias</title>"
    + "    <author>"
    + "      <name>Plato</name>"
    + "    </author>"
    + "    <price>9.99</price>"
    + "  </book>"
    + "</bookstore>";

                string output = "<HTML><BODY><TABLE BORDER=\"2\"><TR><TD>ISBN</TD><TD>Title</TD><TD>Price</TD></TR><TR><TD>1-861003-11-0</TD><TD>The Autobiography of Benjamin Franklin</TD><TD>8.99</TD></TR><TR><TD>0-201-63361-2</TD><TD>The Confidence Man</TD><TD>11.99</TD></TR><TR><TD>1-861001-57-6</TD><TD>The Gorgias</TD><TD>9.99</TD></TR></TABLE></BODY></HTML>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);

                IExecutionContext context = Substitute.For<IExecutionContext>();
                Dictionary<string, string> convertedLinks;
                context.TryConvert(new object(), out convertedLinks)
                    .ReturnsForAnyArgs(x =>
                    {
                        x[1] = x[0];
                        return true;
                    });

                IModule module = Substitute.For<IModule>();
                IDocument xsltDocument = Substitute.For<IDocument>();

                MemoryStream xsltStream = new MemoryStream(Encoding.UTF8.GetBytes(xsltInput));
                xsltDocument.GetStream().Returns(xsltStream);
                context.Execute(Arg.Any<IEnumerable<IModule>>(), Arg.Any<IEnumerable<IDocument>>()).Returns(new IDocument[] {xsltDocument });
                Xslt autoLink = new Xslt(module);

                // When
                autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received().GetDocument(Arg.Is(document), output);
                stream.Dispose();
            }
        }
    }
}
