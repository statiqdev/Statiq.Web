using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Core.Shortcodes.Metadata;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Shortcodes.Metadata
{
    [TestFixture]
    public class MetaFixture : BaseFixture
    {
        public class ExecuteTests : MetaFixture
        {
            [Test]
            public void RendersMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Foo")
                };
                Core.Shortcodes.Metadata.Meta shortcode = new Core.Shortcodes.Metadata.Meta();

                // When
                IShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                using (TextReader reader = new StreamReader(result.Stream))
                {
                    reader.ReadToEnd().ShouldBe("Bar");
                }
            }

            [Test]
            public void NullStreamForMissingMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Fizz")
                };
                Core.Shortcodes.Metadata.Meta shortcode = new Core.Shortcodes.Metadata.Meta();

                // When
                IShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.Stream.ShouldBeNull();
            }
        }
    }
}
