using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class JoinFixture : BaseFixture
    {
        [Test]
        public void JoinTwoDocumentsJoinWithNoDelimiter()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.AreEqual("TestTest2", results.Single().Content);
        }

        [Test]
        public void JoinThreeDocumentsJoinWithNoDelimiter()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new[] { first, second, third }, context).ToList();

            // Then
            Assert.AreEqual("TestTest2Test3", results.Single().Content);
        }

        [Test]
        public void JoinThreeDocumentsJoinWithNoDelimiter_firstnull()
        {
            // Given
            IDocument first = null;
            IDocument second = new TestDocument("Test2");
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new[] { first, second, third }, context).ToList();

            // Then
            Assert.AreEqual("Test2Test3", results.Single().Content);
        }

        [Test]
        public void JoinThreeDocumentsJoinWithNoDelimiter_secondnull()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = null;
            IDocument third = new TestDocument("Test3");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new[] { first, second, third }, context).ToList();

            // Then
            Assert.AreEqual("TestTest3", results.Single().Content);
        }


        [Test]
        public void JoinnullPassedInAsDocumentList()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(null, context).ToList();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }

        [Test]
        public void JoinTwoDocumentsJoinWithCommaDelimiter()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(",");

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.AreEqual("Test,Test2", results.Single().Content);
        }

        [Test]
        public void JoinTwoDocumentsJoinWithDelimiterInText()
        {
            // Given
            IDocument first = new TestDocument("Test");
            IDocument second = new TestDocument("Test2");

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join("Test");

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.AreEqual("TestTestTest2", results.Single().Content);
        }

        [Test]
        public void JoinTwoDocumentsWithKeepFirstMetaDataReturnKeepsFirstMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.FirstDocument);

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.True(results.Single().Keys.Contains("one"));
            Assert.False(results.Single().Keys.Contains("three"));
        }

        [Test]
        public void JoinTwoDocumentsWithMetaDataReturnDefaultMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.False(results.Single().Keys.Contains("one"));
            Assert.False(results.Single().Keys.Contains("three"));
        }

        [Test]
        public void JoinTwoDocumentsWithKeepLastMetaDataReturnKeepsLastMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.LastDocument);

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.True(results.Single().Keys.Contains("three"));
            Assert.False(results.Single().Keys.Contains("one"));
        }

        [Test]
        public void JoinTwoDocumentsWithAllKeepFirstMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "seven"),  new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.AllWithFirstDuplicates);

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.True(results.Single().Values.Contains("two"));
            Assert.False(results.Single().Values.Contains("seven"));

            Assert.True(results.Single().Keys.Contains("three"));
        }

        [Test]
        public void JoinTwoDocumentsWithAllKeepLastMetaData()
        {
            // Given
            IDocument first = new TestDocument("Test", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "two") });
            IDocument second = new TestDocument("Test2", new List<KeyValuePair<string, object>>() { new KeyValuePair<string, object>("one", "seven"), new KeyValuePair<string, object>("three", "four") });

            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(JoinedMetadata.AllWithLastDuplicates);

            // When
            List<IDocument> results = join.Execute(new[] { first, second }, context).ToList();

            // Then
            Assert.False(results.Single().Values.Contains("two"));
            Assert.True(results.Single().Values.Contains("seven"));

            Assert.True(results.Single().Keys.Contains("three"));
        }

        [Test]
        public void EmptyListDoesNotError()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(new IDocument[0], context).ToList();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }

        [Test]
        public void EmptyListWithDelimitorDoesNotError()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join(",");

            // When
            List<IDocument> results = join.Execute(new IDocument[0], context).ToList();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }
    }
}
