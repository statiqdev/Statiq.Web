using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Modules.Contents;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceFixture
    {
        [Test]
        public void RecursiveReplaceWithContentFinder()
        {
            // Given
            string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>foo<span>bar</span></span>
                        </body>
                    </html>";
            string expected = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>baz</span>
                        </body>
                    </html>";
            TestExecutionContext context = new TestExecutionContext();
            TestDocument document = new TestDocument(input);
            Replace replace = new Replace(@"(<span>.*<\/span>)", (Func<Match, object>)(x => "<span>baz</span>"));

            // When
            IList<IDocument> results = replace.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.That(results.First().Content, Is.EquivalentTo(expected));
        }

        [Test]
        public void ReplaceWithContentFinderUsingDocument()
        {
            // Given
            string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>foo<span>bar</span></span>
                        </body>
                    </html>";
            string expected = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <div>Buzz</div>
                        </body>
                    </html>";
            TestExecutionContext context = new TestExecutionContext();
            TestDocument document = new TestDocument(input, new MetadataItems
            {
                { "Fizz", "Buzz" }
            });
            Replace replace = new Replace(@"(<span>.*<\/span>)", (Func<Match, IDocument, object>)((match, doc) => $"<div>{doc["Fizz"]}</div>"));

            // When
            IList<IDocument> results = replace.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.That(results.First().Content, Is.EquivalentTo(expected));
        }
    }
}
