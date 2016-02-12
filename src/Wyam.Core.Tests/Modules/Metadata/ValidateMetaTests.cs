using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ValidateMetaTests : BaseFixture
    {
        public class ExecuteMethodTests : ValidateMetaTests
        {
            /*[Test]
            public void TestForExistenceOfKey()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                document.Metadata.ContainsKey("Title").Returns(true);
                ValidateMeta<string> validateMeta = new ValidateMeta<string>("Title");

                // When
                validateMeta.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.Pass();
            }*/
        }
    }
}
