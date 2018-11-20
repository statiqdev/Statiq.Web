using System;
using NUnit.Framework;
using Wyam.CodeAnalysis.Analysis;
using Wyam.Testing;

namespace Wyam.CodeAnalysis.Tests
{
    [TestFixture]
    public class WrappingStringBuilderFixture : BaseFixture
    {
        public class IntegrationTests : WrappingStringBuilderFixture
        {
            [Test]
            public void DoesNotWrapIfNoBreakpoints()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdefghi", result);
            }

            [Test]
            public void DefaultBehaviorIsNotToWrap()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc");
                builder.Append("def");
                builder.Append("ghi");
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdefghi", result);
            }

            [Test]
            public void WrapsIfBreakpoint()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdef" + Environment.NewLine + "ghi", result);
            }

            [Test]
            public void WrapsEarlierIfEarlierBreakpoint()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", true);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "defghi", result);
            }

            [Test]
            public void NewLinesIncludeNewLinePrefix()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("def", true);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "1234defghi", result);
            }

            [Test]
            public void BreakpointCalculationIncludesNewLinePrefix()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("defxyz", true);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "1234defxyz" + Environment.NewLine + "1234ghi", result);
            }

            [Test]
            public void MultipleWraps()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(2, "1234");

                // When
                builder.Append("abc", true);
                builder.Append("def", true);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "1234def" + Environment.NewLine + "1234ghi", result);
            }

            [Test]
            public void AppendLineBreaksAtEndOfValue()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", false);
                builder.AppendLine("def", false);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdef" + Environment.NewLine + "1234ghi", result);
            }

            [Test]
            public void AppendLineWrapsBeforeValue()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.AppendLine("ghi", true);
                builder.Append("xyz", false);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdef" + Environment.NewLine + "1234ghi" + Environment.NewLine + "1234xyz", result);
            }

            [Test]
            public void MultipleWrapsWithDifferentNewLinePrefixes()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.Append("abc", true);
                builder.Append("def", true);
                builder.NewLinePrefix = "1234";
                builder.Append("ghi", true);
                builder.NewLinePrefix = "5";
                builder.Append("jkl", true);
                builder.Append("m", true);
                builder.Append("n", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "def" + Environment.NewLine + "1234ghi" + Environment.NewLine + "5jklm" + Environment.NewLine + "5n", result);
            }

            [Test]
            public void NoLeadingBreakWhenFirstSegmentIsBreakable()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.Append("abc", true);
                builder.Append("def", false);
                builder.Append("g", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abcdef" + Environment.NewLine + "g", result);
            }

            [Test]
            public void DifferentNewLinePrefixesAfterAppendLine()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.AppendLine("abc", true);
                builder.Append("def", true);
                builder.NewLinePrefix = "1234";
                builder.Append("g", true);
                builder.Append("hij", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "defg" + Environment.NewLine + "1234hij", result);
            }

            [Test]
            public void EmptyAppendLine()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", true);
                builder.AppendLine();
                builder.Append("def", true);
                builder.AppendLine();
                builder.AppendLine();
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                Assert.AreEqual(@"abc" + Environment.NewLine + "1234def" + Environment.NewLine + "1234" + Environment.NewLine + "1234ghi", result);
            }
        }
    }
}
