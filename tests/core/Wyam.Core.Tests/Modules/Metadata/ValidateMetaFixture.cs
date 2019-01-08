using System;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [NonParallelizable]
    public class ValidateMetaFixture : BaseFixture
    {
        public class ExecuteTests : ValidateMetaFixture
        {
            [Test]
            public void ExistenceOfKeyDoesNotThrow()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                Assert.DoesNotThrow(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void AbsenceOfKeyThrows()
            {
                // Given
                IDocument document = new TestDocument();
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                Assert.Throws<AggregateException>(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void FailedAssertionThrows()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Baz");

                // When, Then
                Assert.Throws<AggregateException>(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void PassedAssertionDoesNotThrow()
            {
                // Given
                IDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                IExecutionContext context = new TestExecutionContext();
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Foo");

                // When, Then
                Assert.DoesNotThrow(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }
        }
    }
}
