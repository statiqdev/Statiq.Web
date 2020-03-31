using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Shortcodes;

namespace Statiq.Web.Tests.Shortcodes
{
    [TestFixture]
    public class TableShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : TableShortcodeFixture
        {
            [Test]
            public void RendersTableWithoutSettings()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                string content = @"
1 2 ""3 4""
a ""b c"" d
e f g
5 678
""h i""  j ""k""
l=m nop
";
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[] { };
                TableShortcode shortcode = new TableShortcode();

                // When
                string result = shortcode.Execute(args, content, document, context);

                // Then
                result.ShouldBe(
                    @"<table>
  <tbody>
    <tr>
      <td>1</td>
      <td>2</td>
      <td>3 4</td>
    </tr>
    <tr>
      <td>a</td>
      <td>b c</td>
      <td>d</td>
    </tr>
    <tr>
      <td>e</td>
      <td>f</td>
      <td>g</td>
    </tr>
    <tr>
      <td>5</td>
      <td>678</td>
    </tr>
    <tr>
      <td>h i</td>
      <td>j</td>
      <td>k</td>
    </tr>
    <tr>
      <td>l=m</td>
      <td>nop</td>
    </tr>
  </tbody>
</table>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void RendersTableWithSettings()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                string content = @"
1 2 ""3 4""
a ""b c"" d
e f g
5 678
""h i""  j ""k""
l=m nop
";
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Class", "tclass"),
                    new KeyValuePair<string, string>("HeaderRows", "1"),
                    new KeyValuePair<string, string>("FooterRows", "2"),
                    new KeyValuePair<string, string>("HeaderCols", "1"),
                    new KeyValuePair<string, string>("HeaderClass", "hclass"),
                    new KeyValuePair<string, string>("BodyClass", "bclass"),
                    new KeyValuePair<string, string>("FooterClass", "fclass")
                };
                TableShortcode shortcode = new TableShortcode();

                // When
                string result = shortcode.Execute(args, content, document, context);

                // Then
                result.ShouldBe(
                    @"<table class=""tclass"">
  <thead class=""hclass"">
    <tr>
      <th>1</th>
      <th>2</th>
      <th>3 4</th>
    </tr>
  </thead>
  <tbody class=""bclass"">
    <tr>
      <th>a</th>
      <td>b c</td>
      <td>d</td>
    </tr>
    <tr>
      <th>e</th>
      <td>f</td>
      <td>g</td>
    </tr>
    <tr>
      <th>5</th>
      <td>678</td>
    </tr>
  </tbody>
  <tfoot class=""fclass"">
    <tr>
      <th>h i</th>
      <td>j</td>
      <td>k</td>
    </tr>
    <tr>
      <th>l=m</th>
      <td>nop</td>
    </tr>
  </tfoot>
</table>",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
