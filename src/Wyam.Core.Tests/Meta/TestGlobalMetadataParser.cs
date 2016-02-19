using NUnit.Framework;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Meta
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GlobalMetadataParserTests : BaseFixture
    {
        private GlobalMetadataParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new GlobalMetadataParser();
        }

        public class ParseMethodTests : GlobalMetadataParserTests
        {
            [Test]
            public void TestKeyOnlyParse()
            {
                // Given
                var excepted = new string[] {"hi", "=hello", "\\=abcd", "key\\=val", "     bjorn  \\=   dad"};

                // When
                var args = _parser.Parse(excepted);

                // Then
                Assert.AreEqual(excepted.Length, args.Count);
                int i = 0;
                foreach (var arg in args)
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
                var pairs = new string[]
                {"key=value", "k=v", "except=bro", "awesome====123123", "   keytrimmed    =    value trimmed   "};

                // When
                var args = _parser.Parse(pairs);

                // Then
                Assert.AreEqual(pairs.Length, args.Count);
                foreach (var arg in args)
                {
                    Assert.NotNull(arg.Value, "Argument value should not be null.");
                    StringAssert.DoesNotStartWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", arg.Key, "Arguments key should be trimmed.");
                    StringAssert.DoesNotStartWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                    StringAssert.DoesNotEndWith(" ", (string)arg.Value, "Arguments value should be trimmed.");
                }
            }

            /// <summary>
            /// Same keys are not valid.
            /// </summary>
            [Test]
            public void TestMetadataKeyCollision()
            {
                // Given, When, Then
                Assert.Throws<MetadataParseException>(
                    () => _parser.Parse(new string[] {"hello=world", "hello=exception"}));
            }
        }
    }
}
