using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Shortcodes;
using Wyam.Core.Shortcodes;
using Wyam.Testing;
using Wyam.Testing.Shortcodes;

namespace Wyam.Core.Tests.Shortcodes
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ShortcodeParserFixture : BaseFixture
    {
        public class ParseTests : ShortcodeParserFixture
        {
            [TestCase("<?# name /?>", 0, 11)]
            [TestCase("<?#name/?>", 0, 9)]
            [TestCase("012<?# name /?>xyz", 3, 14)]
            [TestCase("012<?#name/?>xyz", 3, 12)]
            [TestCase("012 <?# name /?> xyz", 4, 15)]
            [TestCase("012 <?#name/?> xyz", 4, 13)]
            [TestCase("012 <?# name /?>xyz", 4, 15)]
            [TestCase("012 <?#name/?>xyz", 4, 13)]
            [TestCase("012<?# name /?> xyz", 3, 14)]
            [TestCase("012<?#name/?> xyz", 3, 12)]
            [TestCase("<?# name foo /?>", 0, 15)]
            [TestCase("<?#name foo/?>", 0, 13)]
            [TestCase("012<?# name foo /?>xyz", 3, 18)]
            [TestCase("012<?#name foo/?>xyz", 3, 16)]
            [TestCase("012 <?# name foo /?> xyz", 4, 19)]
            [TestCase("012 <?#name foo/?> xyz", 4, 17)]
            [TestCase("012 <?# name foo /?>xyz", 4, 19)]
            [TestCase("012 <?#name foo/?>xyz", 4, 17)]
            [TestCase("012<?# name foo /?> xyz", 3, 18)]
            [TestCase("012<?#name foo/?> xyz", 3, 16)]
            public void FindsSelfClosingShortcode(string input, int firstIndex, int lastIndex)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "name", (Type)null }
                    });

                // When
                List<ShortcodeLocation> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [TestCase("<?# name ?><?#/ name ?>", 0, 22)]
            [TestCase("<?#name?><?#/ name ?>", 0, 20)]
            [TestCase("<?# name ?><?#/name?>", 0, 20)]
            [TestCase("<?#name?><?#/name?>", 0, 18)]
            [TestCase("012<?# name ?><?#/ name ?>xyz", 3, 25)]
            [TestCase("012<?# name ?><?#/ name ?> xyz", 3, 25)]
            [TestCase("012 <?# name ?><?#/ name ?>xyz", 4, 26)]
            [TestCase("012 <?# name ?><?#/ name ?> xyz", 4, 26)]
            [TestCase("<?# name ?>abc<?#/ name ?>", 0, 25)]
            [TestCase("<?#name?>abc<?#/ name ?>", 0, 23)]
            [TestCase("<?# name ?>abc<?#/name?>", 0, 23)]
            [TestCase("<?#name?>abc<?#/name?>", 0, 21)]
            [TestCase("012<?# name ?>abc<?#/ name ?>xyz", 3, 28)]
            [TestCase("012<?# name ?>abc<?#/ name ?> xyz", 3, 28)]
            [TestCase("012 <?# name ?>abc<?#/ name ?>xyz", 4, 29)]
            [TestCase("012 <?# name ?>abc<?#/ name ?> xyz", 4, 29)]
            [TestCase("<?# name ?><?#/ foo ?><?#/ name ?>", 0, 33)]
            [TestCase("<?# name ?><?# foo ?><?#/ foo ?><?#/ name ?>", 0, 43)]
            [TestCase("<?# name ?>abc<?#/ foo ?>xyz<?#/ name ?>", 0, 39)]
            [TestCase("<?# name ?>abc<?# foo ?>def<?#/ foo ?>xyz<?#/ name ?>", 0, 52)]
            [TestCase("<?# name ?>{{!/ foo !}}<?#/ name ?>", 0, 34)]
            [TestCase("<?# name ?>{{! foo !}}{{!/ foo !}}<?#/ name ?>", 0, 45)]
            [TestCase("<?# name ?>{{! foo !}}{{*/ foo *}}<?#/ name ?>", 0, 45)]
            [TestCase("<?# name ?>abc{{* foo *}}def{{*/ foo *}}xyz<?#/ name ?>", 0, 54)]
            public void FindsClosingShortcode(string input, int firstIndex, int lastIndex)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "name", (Type)null }
                    });

                // When
                List<ShortcodeLocation> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [TestCase("<?# foo ?>")]
            [TestCase("<?#foo?>")]
            [TestCase("abc<?# foo ?>123")]
            [TestCase("abc<?# foo ?>123<?# bar ?>")]
            [TestCase("abc<?# foo ?>123<?#/ foo ?>456<?# bar ?>")]
            [TestCase("abc<?# foo ?>123<?# bar ?><?#/ bar ?>")]
            public void ThrowsForUnterminatedShortcode(string input)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "foo", (Type)null },
                        { "bar", (Type)null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [TestCase("{{ name }}{{/ name }}", 0, 20, "{{", "}}")]
            [TestCase("{{name}}{{/ name }}", 0, 18, "{{", "}}")]
            [TestCase("{{ name }}abc{{/ name }}", 0, 23, "{{", "}}")]
            [TestCase("012{{ name }}abc{{/ name }}xyz", 3, 26, "{{", "}}")]
            [TestCase("{{ name }}abc<?#/ foo ?>xyz{{/ name }}", 0, 37, "{{", "}}")]
            [TestCase("{{ name }}abc<?# foo ?>def<?#/ foo ?>xyz{{/ name }}", 0, 50, "{{", "}}")]
            [TestCase("{{ name }}{{!/ foo !}}{{/ name }}", 0, 32, "{{", "}}")]
            [TestCase("{ name }{/ name }", 0, 16, "{", "}")]
            [TestCase("{name}{/ name }", 0, 14, "{", "}")]
            [TestCase("{ name }abc{/ name }", 0, 19, "{", "}")]
            [TestCase("012{ name }abc{/ name }xyz", 3, 22, "{", "}")]
            [TestCase("{ name }abc<?#/ foo ?>xyz{/ name }", 0, 33, "{", "}")]
            [TestCase("{ name }abc<?# foo ?>def<?#/ foo ?>xyz{/ name }", 0, 46, "{", "}")]
            [TestCase("{ name }{{!/ foo !}}{/ name }", 0, 28, "{", "}")]
            [TestCase("| name ||/ name |", 0, 16, "|", "|")]
            [TestCase("|name||/ name |", 0, 14, "|", "|")]
            [TestCase("| name |abc|/ name |", 0, 19, "|", "|")]
            [TestCase("012| name |abc|/ name |xyz", 3, 22, "|", "|")]
            [TestCase("| name |abc|/ foo |xyz|/ name |", 0, 30, "|", "|")]
            [TestCase("| name |abc| foo |def|/ foo |xyz|/ name |", 0, 40, "|", "|")]
            [TestCase("| name ||/ foo ||/ name |", 0, 24, "|", "|")]
            public void SupportsAlternateDelimiters(
                string input,
                int firstIndex,
                int lastIndex,
                string startDelimiter,
                string endDelimiter)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    startDelimiter,
                    endDelimiter,
                    new TestShortcodeCollection
                    {
                        { "name", (Type)null }
                    });

                // When
                List<ShortcodeLocation> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [TestCase("<?# name ?>abc<?#/ name ?>", "abc")]
            [TestCase("<?#name?>abc<?#/ name ?>", "abc")]
            [TestCase("<?# name ?>abc<?#/name?>", "abc")]
            [TestCase("<?#name?>abc<?#/name?>", "abc")]
            [TestCase("012<?# name ?>abc<?#/ name ?>xyz", "abc")]
            [TestCase("012<?# name ?>abc<?#/ name ?> xyz", "abc")]
            [TestCase("012 <?# name ?>abc<?#/ name ?>xyz", "abc")]
            [TestCase("012 <?# name ?>abc<?#/ name ?> xyz", "abc")]
            [TestCase("<?# name ?>abc<?#/ foo ?>xyz<?#/ name ?>", "abc<?#/ foo ?>xyz")]
            [TestCase("<?# name ?>abc<?# foo ?>def<?#/ foo ?>xyz<?#/ name ?>", "abc<?# foo ?>def<?#/ foo ?>xyz")]
            [TestCase("<?# name ?>{{!/ foo !}}<?#/ name ?>", "{{!/ foo !}}")]
            [TestCase("<?# name ?>{{! foo !}}{{!/ foo !}}<?#/ name ?>", "{{! foo !}}{{!/ foo !}}")]
            [TestCase("<?# name ?>{{! foo !}}{{*/ foo *}}<?#/ name ?>", "{{! foo !}}{{*/ foo *}}")]
            [TestCase("<?# name ?>abc{{* foo *}}def{{*/ foo *}}xyz<?#/ name ?>", "abc{{* foo *}}def{{*/ foo *}}xyz")]
            public void FindsContent(string input, string content)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "name", (Type)null }
                    });

                // When
                List<ShortcodeLocation> result = parser.Parse(stream);

                // Then
                result.Single().Content.ShouldBe(content);
            }

            [TestCase("<?# name ?><?*abc?><?#/ name ?>", "abc")]
            [TestCase("<?# name ?><?* abc ?><?#/ name ?>", " abc ")]
            [TestCase("<?# name ?> <?*abc?> <?#/ name ?>", "abc")]
            [TestCase("<?# name ?> <?* abc ?> <?#/ name ?>", " abc ")]
            [TestCase("<?# name ?><?#/ foo ?><?#/ name ?>", "<?#/ foo ?>")]
            [TestCase("<?# name ?><?# foo ?><?#/ foo ?><?#/ name ?>", "<?# foo ?><?#/ foo ?>")]
            [TestCase("<?# name ?><?*<?#/ foo ?>?><?#/ name ?>", "<?#/ foo ?>")]
            [TestCase("<?# name ?><?*<?# foo ?><?#/ foo ?>?><?#/ name ?>", "<?# foo ?><?#/ foo ?>")]
            public void TrimsWildcardProcessingInstructionFromContent(string input, string content)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "name", (Type)null }
                    });

                // When
                List<ShortcodeLocation> result = parser.Parse(stream);

                // Then
                result.Single().Content.ShouldBe(content);
            }

            [Test]
            public void ThrowsForUnnamedShortcodeName()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("<?# ?>abc<?#/ ?>"));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "bar", (Type)null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [Test]
            public void ThrowsForUnregisteredShortcodeName()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("<?# foo ?>abc<?#/ foo ?>"));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "bar", (Type)null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [Test]
            public void ThrowsForExtraClosingContent()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("<?# foo ?>abc<?#/ foo bar ?>"));
                ShortcodeParser parser = new ShortcodeParser(
                    ShortcodeParser.DefaultPostRenderStartDelimiter,
                    ShortcodeParser.DefaultPostRenderEndDelimiter,
                    new TestShortcodeCollection
                    {
                        { "foo", (Type)null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }
        }

        public class SplitArgumentsTests : ShortcodeParserFixture
        {
            [Test]
            public void ShouldIgnoreLeadingAndTrailingWhiteSpace()
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeParser.SplitArguments("  foo  fizz=buzz  ", 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "foo"),
                    new KeyValuePair<string, string>("fizz", "buzz")
                });
            }

            [TestCase("foo", null, "foo")]
            [TestCase("=foo", null, "foo")]
            [TestCase("foo=bar", "foo", "bar")]
            [TestCase("\"fizz buzz\"=bar", "fizz buzz", "bar")]
            [TestCase("foo=\"bar baz\"", "foo", "bar baz")]
            [TestCase("\"fizz buzz\"=\"bar baz\"", "fizz buzz", "bar baz")]
            [TestCase("\"fizz \\\" buzz\"=bar", "fizz \" buzz", "bar")]
            [TestCase("foo=\"bar \\\" baz\"", "foo", "bar \" baz")]
            [TestCase("\"fizz \\\" buzz\"=\"bar \\\" baz\"", "fizz \" buzz", "bar \" baz")]
            [TestCase("\"fizz = buzz\"=bar", "fizz = buzz", "bar")]
            [TestCase("foo=\"bar = baz\"", "foo", "bar = baz")]
            [TestCase("\"fizz = buzz\"=\"bar = baz\"", "fizz = buzz", "bar = baz")]
            public void ShouldSplitArguments(string arguments, string expectedKey, string expectedValue)
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeParser.SplitArguments(arguments, 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(expectedKey, expectedValue)
                });
            }

            [Test]
            public void ShouldSplitComplexArguments()
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeParser.SplitArguments("foo \"abc 123\" fizz=buzz  \"qwe\"=\"try\"\r\nxyz=\"zyx\"  \"678=987\" goo=boo", 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "foo"),
                    new KeyValuePair<string, string>(null, "abc 123"),
                    new KeyValuePair<string, string>("fizz", "buzz"),
                    new KeyValuePair<string, string>("qwe", "try"),
                    new KeyValuePair<string, string>("xyz", "zyx"),
                    new KeyValuePair<string, string>(null, "678=987"),
                    new KeyValuePair<string, string>("goo", "boo")
                });
            }
        }
    }
}
