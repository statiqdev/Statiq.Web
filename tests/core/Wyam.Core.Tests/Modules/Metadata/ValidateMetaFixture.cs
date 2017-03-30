using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ValidateMetaFixture : BaseFixture
    {
        public class ExecuteTests : ValidateMetaFixture
        {
            [Test]
            public void ExistenceOfKeyDoesNotThrow()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(true);
                string value;
                document.MetadataAs<string>().TryGetValue("Title", out value).Returns(true);
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                Assert.DoesNotThrow(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void AbsenceOfKeyThrows()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(false);
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                Assert.Throws<AggregateException>(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void CanNotConvertThrows()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(true);
                string value;
                document.MetadataAs<string>().TryGetValue("Title", out value).Returns(false);
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When, Then
                Assert.Throws<AggregateException>(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void FailedAssertionThrows()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(true);
                string value;
                document.MetadataAs<string>().TryGetValue("Title", out value).Returns(x =>
                {
                    x[1] = "Foobar";
                    return true;
                });
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Baz");

                // When, Then
                Assert.Throws<AggregateException>(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }

            [Test]
            public void PassedAssertionDoesNotThrow()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(true);
                string value;
                document.MetadataAs<string>().TryGetValue("Title", out value).Returns(x =>
                {
                    x[1] = "Foobar";
                    return true;
                });
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title").WithAssertion(x => x == "Foobar");

                // When, Then
                Assert.DoesNotThrow(() => validateMeta.Execute(new[] { document }, context).ToList());  // Make sure to materialize the result list
            }
        }
    }
}
