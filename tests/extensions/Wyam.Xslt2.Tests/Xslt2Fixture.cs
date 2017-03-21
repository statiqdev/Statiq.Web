using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Xslt2.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class Xslt2Fixture : BaseFixture
    {
        public class ExecuteTests : Xslt2Fixture
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

                string output =
         "<HTML>"
+ "\n" + "   <BODY>"
+ "\n" + "      <TABLE BORDER=\"2\">"
+ "\n" + "         <TR>"
+ "\n" + "            <TD>ISBN</TD>"
+ "\n" + "            <TD>Title</TD>"
+ "\n" + "            <TD>Price</TD>"
+ "\n" + "         </TR>"
+ "\n" + "         <TR>"
+ "\n" + "            <TD>1-861003-11-0</TD>"
+ "\n" + "            <TD>The Autobiography of Benjamin Franklin</TD>"
+ "\n" + "            <TD>8.99</TD>"
+ "\n" + "         </TR>"
+ "\n" + "         <TR>"
+ "\n" + "            <TD>0-201-63361-2</TD>"
+ "\n" + "            <TD>The Confidence Man</TD>"
+ "\n" + "            <TD>11.99</TD>"
+ "\n" + "         </TR>"
+ "\n" + "         <TR>"
+ "\n" + "            <TD>1-861001-57-6</TD>"
+ "\n" + "            <TD>The Gorgias</TD>"
+ "\n" + "            <TD>9.99</TD>"
+ "\n" + "         </TR>"
+ "\n" + "      </TABLE>"
+ "\n" + "   </BODY>"
+ "\n" + "</HTML>";

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                TestDocument xsltDocument = new TestDocument(xsltInput);
                IModule module = Substitute.For<IModule>();
                module.Execute(Arg.Any<IReadOnlyList<IDocument>>(), Arg.Any<IExecutionContext>()).Returns(new IDocument[] { xsltDocument });
                Xslt2 xslt2 = new Xslt2(module);

                // When
                IList<IDocument> results = xslt2.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void TestTransform2()
            {
                // Given
                string xsltInput = ""
    + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    + "\n" + "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" >"
    + "\n" + "<xsl:output method=\"xml\" indent=\"yes\" />"
    + "\n" + ""
    + "\n" + "<xsl:template match=\"world\">"
    + "\n" + "    <div>"
    + "\n" + "        <xsl:for-each-group select=\"country\" group-by=\"@continent\">"
    + "\n" + "            <div>"
    + "\n" + "		<h1><xsl:value-of select=\"@continent\" /></h1>"
    + "\n" + "                <xsl:for-each select=\"current-group()\">"
    + "\n" + "                    <p>"
    + "\n" + "                        <xsl:value-of select=\"@name\" />"
    + "\n" + "                    </p>"
    + "\n" + "                </xsl:for-each>"
    + "\n" + "            </div>"
    + "\n" + "        </xsl:for-each-group>"
    + "\n" + "    </div>"
    + "\n" + "</xsl:template>"
    + "\n" + "</xsl:stylesheet>";




                string input = ""
    + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    + "<world>"
    + "<country name=\"Canada\" continent=\"North America\" />"
    + "<country name=\"Jamaica\" continent=\"North America\" />"
    + "<country name=\"United States\" continent=\"North America\" />"
    + "<country name=\"United Kingdom\" continent=\"Europe\" />"
    + "<country name=\"France\" continent=\"Europe\" />"
    + "<country name=\"Japan\" continent=\"Asia\" />"
    + "</world>";


                string output = ""
     + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
    + "\n" + "<div>"
    + "\n" + "   <div>"
    + "\n" + "      <h1>North America</h1>"
    + "\n" + "      <p>Canada</p>"
    + "\n" + "      <p>Jamaica</p>"
    + "\n" + "      <p>United States</p>"
    + "\n" + "   </div>"
    + "\n" + "   <div>"
    + "\n" + "      <h1>Europe</h1>"
    + "\n" + "      <p>United Kingdom</p>"
    + "\n" + "      <p>France</p>"
    + "\n" + "   </div>"
    + "\n" + "   <div>"
    + "\n" + "      <h1>Asia</h1>"
    + "\n" + "      <p>Japan</p>"
    + "\n" + "   </div>"
    + "\n" + "</div>"
    + "\n" ;


                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                TestDocument xsltDocument = new TestDocument(xsltInput);
                IModule module = Substitute.For<IModule>();
                module.Execute(Arg.Any<IReadOnlyList<IDocument>>(), Arg.Any<IExecutionContext>()).Returns(new IDocument[] { xsltDocument });
                Xslt2 xslt2 = new Xslt2(module);

                // When
                IList<IDocument> results = xslt2.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }
        }
    }
}
