using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Common.Execution;

namespace Wyam.Core.Tests.Execution
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PipelineCollectionFixture : BaseFixture
    {
        public class AddTests : PipelineCollectionFixture
        {
            [TestCase(null)]
            [TestCase("")]
            public void AutoGeneratesNameForNullOrEmpty(string name)
            {
                // Given
                PipelineCollection pipelines = new PipelineCollection();
                pipelines.Add("First");

                // When
                pipelines.Add(name);

                // Then
                Assert.AreEqual("Pipeline 2", pipelines.Values.Skip(1).First().Name);
            }

            [Test]
            public void ThrowsForDuplicateName()
            {
                // Given
                PipelineCollection pipelines = new PipelineCollection();
                pipelines.Add("Foo");

                // When, Then
                Assert.Throws<ArgumentException>(() => pipelines.Add("Foo"));
            }

            [Test]
            public void ThrowsForDuplicateNameWithDifferentCase()
            {
                // Given
                PipelineCollection pipelines = new PipelineCollection();
                pipelines.Add("Foo");

                // When, Then
                Assert.Throws<ArgumentException>(() => pipelines.Add("foo"));
            }
        }

        public class ContainsKeyTests : PipelineCollectionFixture
        {
            [Test]
            public void ReturnsTrueForDifferentCase()
            {
                // Given
                PipelineCollection pipelines = new PipelineCollection();
                pipelines.Add("Test");

                // When
                bool contains = pipelines.ContainsKey("test");

                // Then
                Assert.IsTrue(contains);
            }
        }
    }
}
