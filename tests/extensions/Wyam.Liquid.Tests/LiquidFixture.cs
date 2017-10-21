using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Liquid;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Liquid.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class LiquidFixture : BaseFixture
    {
        public class ExecuteTests : LiquidFixture
        {
            [Test]
            public void RendersLiquid()
            {
                // Given
                string input = @"<div>{{ 'liquid' | upcase }}</div>
<div>liquid has {{ 'liquid' | size }} letters!</div>
<div>{{ '11/1/2017' | date:'MMMM dd, yyyy' }}</div>
".Replace(Environment.NewLine, "\n");
                string output = @"<div>LIQUID</div>
<div>liquid has 6 letters!</div>
<div>November 01, 2017</div>
".Replace(Environment.NewLine, "\n");

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Liquid markdown = new Liquid();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }


        }
    }
}