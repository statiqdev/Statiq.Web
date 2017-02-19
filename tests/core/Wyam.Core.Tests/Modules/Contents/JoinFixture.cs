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
        public void TwoDocumentsJoinWithNoDelimiter()
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
        public void ThreeDocumentsJoinWithNoDelimiter()
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
        public void ThreeDocumentsJoinWithNoDelimiter_firstnull()
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
        public void ThreeDocumentsJoinWithNoDelimiter_secondnull()
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
        public void nullPassedInAsDocumentList()
        {
            // Given
            IExecutionContext context = new TestExecutionContext();
            Join join = new Join();

            // When
            List<IDocument> results = join.Execute(null, context).ToList();

            // Then
            Assert.AreEqual(null, results.Single().Content);
        }



    }
}

/*
 * TODO metadata unit test 

- how should it work


based on first file?
or be all files?

or should it be default

 * 
 * */
