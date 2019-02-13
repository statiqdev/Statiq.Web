using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Common.Tests.Shortcodes
{
    [TestFixture]
    public class ShortcodeHelperFixture : BaseFixture
    {
        public class GetArgsDictionaryTests : ShortcodeHelperFixture
        {
            [Test]
            public void MatchesInCorrectOrder()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>(null, "2"),
                    new KeyValuePair<string, string>(null, "3")
                };

                // When
                IMetadataDictionary dictionary = ShortcodeHelper.GetArgsDictionary(context, args, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("B", "2"),
                    new KeyValuePair<string, string>("A", "1"),
                    new KeyValuePair<string, string>("C", "3")
                };

                // When
                IMetadataDictionary dictionary = ShortcodeHelper.GetArgsDictionary(context, args, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesPositionalAndNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("B", "2")
                };

                // When
                IMetadataDictionary dictionary = ShortcodeHelper.GetArgsDictionary(context, args, "A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void ThrowsForPositionalAfterNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>(null, "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => ShortcodeHelper.GetArgsDictionary(context, args, "A", "B", "C"));
            }

            [Test]
            public void ThrowsForDuplicateNamedArguments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("A", "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => ShortcodeHelper.GetArgsDictionary(context, args, "A", "B", "C"));
            }
        }
    }
}
