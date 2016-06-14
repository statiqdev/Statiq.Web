using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Testing;

namespace Wyam.Core.Tests.Execution
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PipelineCollectionTests : BaseFixture
    {
        public class AddMethodTests : PipelineCollectionTests
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
                Assert.AreEqual("Pipeline 2", pipelines.Skip(1).First().Key);
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

        public class ContainsKeyMethodTests : PipelineCollectionTests
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
