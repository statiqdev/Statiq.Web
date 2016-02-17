using System;
using NUnit.Framework;
using Wyam.Core.Util;

namespace Wyam.Core.Tests
{
    public class TestGlobalMetadataParser
    {
        GlobalMetadataParser parser;

        [SetUp]
        public void SetUp()
        {
            parser = new GlobalMetadataParser();
        }

        [Test]
        public void TestKeyOnlyParse()
        {
            var excepted = new string[] { "hi", "=hello", "\\=abcd", "key\\=val", "     bjorn  \\=   dad" };

            var args = parser.Parse(excepted);
            Assert.AreEqual(excepted.Length, args.Count);

            int i = 0;
            foreach (var arg in args) {
                Assert.AreEqual(excepted[i].Replace("\\=", "=").Trim(), arg.Key);
                Assert.IsNull(arg.Value);
                i++;
            }
        }

        [Test]
        public void TestKeyValueParse()
        {
            var pairs = new string[] { "key=value", "k=v", "except=bro", "awesome====123123", "   keytrimmed    =    value trimmed   " };
            var args = parser.Parse(pairs);
            Assert.AreEqual(pairs.Length, args.Count);

            foreach (var arg in args)
            {
                Assert.NotNull(arg.Value, "Argument value should not be null.");
                StringAssert.DoesNotStartWith(" ", arg.Key, "Arguments key should be trimmed.");
                StringAssert.DoesNotEndWith(" ", arg.Key, "Arguments key should be trimmed.");
                StringAssert.DoesNotStartWith(" ", arg.Value, "Arguments value should be trimmed.");
                StringAssert.DoesNotEndWith(" ", arg.Value, "Arguments value should be trimmed.");
            }
        }

        /// <summary>
        /// Same keys are not valid.
        /// </summary>
        [Test]
        public void TestMetadataKeyCollision()
        {
            Assert.Throws<MetadataParseException>(() => parser.Parse(new string[] { "hello=world", "hello=exception" }));
        }
    }
}
