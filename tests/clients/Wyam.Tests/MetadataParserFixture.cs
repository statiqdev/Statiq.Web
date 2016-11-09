using System;
using System.Collections.Generic;
using NUnit.Framework;
using Wyam.Testing;

namespace Wyam.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataParserFixture : BaseFixture
    {
        public class ParseTests : MetadataParserFixture
        {
            [Test]
            public void TestKeyOnlyParse()
            {
                // Given
                string[] excepted = {"hi", "=hello", "\\=abcd", "key\\=val", "     bjorn  \\=   dad"};

                // When
                IReadOnlyDictionary<string, object> args = MetadataParser.Parse(excepted);

                // Then
                Assert.AreEqual(excepted.Length, args.Count);
                int i = 0;
                foreach (KeyValuePair<string, object> arg in args)
                {
                    Assert.AreEqual(excepted[i].Replace("\\=", "=").Trim(), arg.Key);
                    Assert.IsNull(arg.Value);
                    i++;
                }
            }

            [Test]
            public void TestKeyValueParse()
            {
                // Given
                string[] pairs = {"key=value", "k=v", "except=bro", "awesome====123123", "   keytrimmed    =    value trimmed   "};

                // When
                IReadOnlyDictionary<string, object> args = MetadataParser.Parse(pairs);

                // Then
                Assert.AreEqual(pairs.Length, args.Count);
                foreach (KeyValuePair<string, object> arg in args)
                {
                    Assert.NotNull(arg.Value, "Argument value should not be null.");
                    StringAssert.DoesNotStartWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotStartWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                }
            }

            [Test]
            public void ArrayValue()
            {
                // Given
                string[] pairs = {"foo = [bar,baz boo,baz\\, boo, a, b\\\"b]"};

                // When
                IReadOnlyDictionary<string, object> args = MetadataParser.Parse(pairs);

                // Then
                CollectionAssert.AreEqual(new [] {"foo"}, args.Keys);
                CollectionAssert.AreEqual(new [] {"bar", "baz boo", "baz, boo", "a", "b\"b"}, args["foo"] as object[]);
            }

            /// <summary>
            /// Same keys are not valid.
            /// </summary>
            [Test]
            public void TestMetadataKeyCollision()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(
                    () => MetadataParser.Parse(new [] {"hello=world", "hello=exception"}));
            }
        }
    }
}
