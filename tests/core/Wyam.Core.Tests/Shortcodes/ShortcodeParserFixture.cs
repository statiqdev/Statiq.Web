using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Core.Shortcodes;
using Wyam.Testing;

namespace Wyam.Core.Tests.Shortcodes
{
    [TestFixture]
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
                    "{{",
                    "}}",
                    new Dictionary<string, Common.Shortcodes.IShortcode>
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
            public void FindsClosingShortcode(string input, int firstIndex, int lastIndex)
            {
                // Given
                Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                ShortcodeParser parser = new ShortcodeParser(
                    "{{",
                    "}}",
                    new Dictionary<string, Common.Shortcodes.IShortcode>
                    {
                        { "name", null }
                    });

                // When
                List<ShortcodeInstance> result = parser.Parse(stream);

                // Then
                result.Single().FirstIndex.ShouldBe(firstIndex);
                result.Single().LastIndex.ShouldBe(lastIndex);
            }

            // Does not find different close name

            // Finds different styles

            // "{{ % %}}"
            // "{{% % }}" - also test space between style in end tag
            // "{{%%}}"
            // "{{% %}}"
            // "{{% name %}}{{%/ %}}

            // Throws for unterminated shortcode
            // "{{% name %}}"
            // "{{%name%}}"
            // "...{{% name %}}..."
        }
    }
}
