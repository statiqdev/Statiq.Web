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
            [TestCase("{{% name /%}}", 0, 12)]
            [TestCase("{{%name/%}}", 0, 10)]
            [TestCase("012{{% name /%}}xyz", 3, 15)]
            [TestCase("012{{%name/%}}xyz", 3, 13)]
            [TestCase("012 {{% name /%}} xyz", 4, 16)]
            [TestCase("012 {{%name/%}} xyz", 4, 14)]
            [TestCase("012 {{% name /%}}xyz", 4, 16)]
            [TestCase("012 {{%name/%}}xyz", 4, 14)]
            [TestCase("012{{% name /%}} xyz", 3, 15)]
            [TestCase("012{{%name/%}} xyz", 3, 13)]
            [TestCase("{{% name foo /%}}", 0, 16)]
            [TestCase("{{%name foo/%}}", 0, 14)]
            [TestCase("012{{% name foo /%}}xyz", 3, 19)]
            [TestCase("012{{%name foo/%}}xyz", 3, 17)]
            [TestCase("012 {{% name foo /%}} xyz", 4, 20)]
            [TestCase("012 {{%name foo/%}} xyz", 4, 18)]
            [TestCase("012 {{% name foo /%}}xyz", 4, 20)]
            [TestCase("012 {{%name foo/%}}xyz", 4, 18)]
            [TestCase("012{{% name foo /%}} xyz", 3, 19)]
            [TestCase("012{{%name foo/%}} xyz", 3, 17)]
            public void FindsSelfClosingShortcode(string input, int firstIndex, int lastIndex)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "name", null }
                    });

                // When
                List<ShortcodeInstance> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [TestCase("{{% name %}}{{%/ name %}}", 0, 24)]
            [TestCase("{{%name%}}{{%/ name %}}", 0, 22)]
            [TestCase("{{% name %}}{{%/name%}}", 0, 22)]
            [TestCase("{{%name%}}{{%/name%}}", 0, 20)]
            [TestCase("012{{% name %}}{{%/ name %}}xyz", 3, 27)]
            [TestCase("012{{% name %}}{{%/ name %}} xyz", 3, 27)]
            [TestCase("012 {{% name %}}{{%/ name %}}xyz", 4, 28)]
            [TestCase("012 {{% name %}}{{%/ name %}} xyz", 4, 28)]
            [TestCase("{{% name %}}abc{{%/ name %}}", 0, 27)]
            [TestCase("{{%name%}}abc{{%/ name %}}", 0, 25)]
            [TestCase("{{% name %}}abc{{%/name%}}", 0, 25)]
            [TestCase("{{%name%}}abc{{%/name%}}", 0, 23)]
            [TestCase("012{{% name %}}abc{{%/ name %}}xyz", 3, 30)]
            [TestCase("012{{% name %}}abc{{%/ name %}} xyz", 3, 30)]
            [TestCase("012 {{% name %}}abc{{%/ name %}}xyz", 4, 31)]
            [TestCase("012 {{% name %}}abc{{%/ name %}} xyz", 4, 31)]
            [TestCase("{{% name %}}{{%/ foo %}}{{%/ name %}}", 0, 36)]
            [TestCase("{{% name %}}{{% foo %}}{{%/ foo %}}{{%/ name %}}", 0, 47)]
            [TestCase("{{% name %}}abc{{%/ foo %}}xyz{{%/ name %}}", 0, 42)]
            [TestCase("{{% name %}}abc{{% foo %}}def{{%/ foo %}}xyz{{%/ name %}}", 0, 56)]
            [TestCase("{{% name %}}{{!/ foo !}}{{%/ name %}}", 0, 36)]
            [TestCase("{{% name %}}{{! foo !}}{{!/ foo !}}{{%/ name %}}", 0, 47)]
            [TestCase("{{% name %}}{{! foo !}}{{*/ foo *}}{{%/ name %}}", 0, 47)]
            [TestCase("{{% name %}}abc{{* foo *}}def{{*/ foo *}}xyz{{%/ name %}}", 0, 56)]
            public void FindsClosingShortcode(string input, int firstIndex, int lastIndex)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "name", null }
                    });

                // When
                List<ShortcodeInstance> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [TestCase("{{% foo %}}")]
            [TestCase("{{%foo%}}")]
            [TestCase("abc{{% foo %}}123")]
            [TestCase("abc{{% foo %}}123{{% bar %}}")]
            [TestCase("abc{{% foo %}}123{{%/ foo %}}456{{% bar %}}")]
            [TestCase("abc{{% foo %}}123{{% bar %}}{{%/ bar %}}")]
            public void ThrowsForUnterminatedShortcode(string input)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "foo", null },
                        { "bar", null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [TestCase("{{ name }}{{/ name }}", 0, 20, "{{", "}}")]
            [TestCase("{{name}}{{/ name }}", 0, 18, "{{", "}}")]
            [TestCase("{{ name }}abc{{/ name }}", 0, 23, "{{", "}}")]
            [TestCase("012{{ name }}abc{{/ name }}xyz", 3, 26, "{{", "}}")]
            [TestCase("{{ name }}abc{{%/ foo %}}xyz{{/ name }}", 0, 38, "{{", "}}")]
            [TestCase("{{ name }}abc{{% foo %}}def{{%/ foo %}}xyz{{/ name }}", 0, 52, "{{", "}}")]
            [TestCase("{{ name }}{{!/ foo !}}{{/ name }}", 0, 32, "{{", "}}")]
            [TestCase("{ name }{/ name }", 0, 16, "{", "}")]
            [TestCase("{name}{/ name }", 0, 14, "{", "}")]
            [TestCase("{ name }abc{/ name }", 0, 19, "{", "}")]
            [TestCase("012{ name }abc{/ name }xyz", 3, 22, "{", "}")]
            [TestCase("{ name }abc{{%/ foo %}}xyz{/ name }", 0, 34, "{", "}")]
            [TestCase("{ name }abc{{% foo %}}def{{%/ foo %}}xyz{/ name }", 0, 48, "{", "}")]
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
                        { "name", null }
                    });

                // When
                List<ShortcodeInstance> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            [Test]
            public void ThrowsForUnnamedShortcodeName()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("{{% %}}abc{{%/ %}}"));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "bar", null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [Test]
            public void ThrowsForUnregisteredShortcodeName()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("{{% foo %}}abc{{%/ foo %}}"));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "bar", null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }

            [Test]
            public void ThrowsForExtraClosingContent()
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes("{{% foo %}}abc{{%/ foo bar %}}"));
                ShortcodeParser parser = new ShortcodeParser(
                    new TestShortcodeCollection
                    {
                        { "foo", null }
                    });

                // When, Then
                Should.Throw<ShortcodeParserException>(() => parser.Parse(stream));
            }
        }
    }
}
