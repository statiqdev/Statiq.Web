using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class AutoLinkFixture : BaseFixture
    {
        public class ExecuteTests : AutoLinkFixture
        {
            [Test]
            public void NoReplacementReturnsSameDocument()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobaz", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().ShouldBeSameAs(document);
            }

            [Test]
            public void AddsLink()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.google.com"">Foobar</a> text</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsLinkWithoutImpactingEscapes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This A&lt;string, List&lt;B&gt;&gt; is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This A&lt;string, List&lt;B&gt;&gt; is some <a href=""http://www.google.com"">Foobar</a> text</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsLinkWithAlternateQuerySelector()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some Foobar text</baz>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some <a href=""http://www.google.com"">Foobar</a> text</baz>
                            <p>This is some Foobar text</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                }).WithQuerySelector("baz");

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsLinkWhenContainerHasChildElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some Foobar <b>text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <a href=""http://www.google.com"">Foobar</a> <b>text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsLinkWhenInsideChildElement()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i>Foobar</i> <b>text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i><a href=""http://www.google.com"">Foobar</a></i> <b>text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotReplaceInAttributes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some Foobar <b ref=""Foobar"">text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some <a href=""http://www.google.com"">Foobar</a> <b ref=""Foobar"">text</b></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsMultipleLinksInSameElement()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsMultipleLinksInDifferentElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                            <p>Another Foobaz paragraph</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                            <p>Another <a href=""http://www.bing.com"">Foobaz</a> paragraph</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotAddLinksInExistingLinkElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddsMultipleLinksWhenFirstIsSubstring()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a>bar</i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AddLinkMethodTakesPrecedence()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.yahoo.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                }).WithLink("Foobaz", "http://www.yahoo.com");

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void IgnoreSubstringIfSearchingForWholeWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foo</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a></i> text Foobaz</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                }).WithMatchOnlyWholeWord();

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AdjacentWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc <a href=""http://www.google.com"">Foo</a>(<a href=""http://www.yahoo.com"">baz</a>) xyz</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                });

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void WordAtTheEndAfterPreviousWord()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>sdfg asdf aasdf asf asdf asdf asdf aabc Fuzz bazz def efg baz x</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>sdfg asdf aasdf asf asdf asdf asdf aabc <a href=""http://www.google.com"">Fuzz</a> bazz def efg <a href=""http://www.yahoo.com"">baz</a> x</p>
                        
                    </body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Fuzz", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                }).WithMatchOnlyWholeWord();

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void NonWholeWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                }).WithMatchOnlyWholeWord();

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().ShouldBeSameAs(document);
            }

            [TestCase("<li>Foo</li>", "<li>Foo</li>")]
            [TestCase("<li>Foo&lt;T&gt;</li>", "<li>Foo&lt;T&gt;</li>")]
            [TestCase("<li><code>Foo</code></li>", @"<li><code><a href=""http://www.foo.com"">Foo</a></code></li>")]
            [TestCase("<li><code>Foo&lt;T&gt;</code></li>", @"<li><code><a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a></code></li>")]
            [TestCase("<li><code>Foo&lt;Foo&gt;</code></li>", @"<li><code>Foo&lt;<a href=""http://www.foo.com"">Foo</a>&gt;</code></li>")]
            [TestCase("<li><code>Foo&lt;Foo&lt;T&gt;&gt;</code></li>", @"<li><code>Foo&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;Foo&gt;</code></li>", @"<li><code>IEnumerable&lt;<a href=""http://www.foo.com"">Foo</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;Foo&lt;T&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;IEnumerable&lt;Foo&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;IEnumerable&lt;<a href=""http://www.foo.com"">Foo</a>&gt;&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;IEnumerable&lt;Foo&lt;T&gt;&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;IEnumerable&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;&gt;</code></li>")]
            public void AddLinksToGenericWordsInsideAngleBrackets(string input, string expected)
            {
                // Given
                string inputContent = $"<html><head></head><body><foo></foo><ul>{input}</ul></body></html>";
                string expectedContent = $"<html><head></head><body><foo></foo><ul>{expected}</ul></body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>()
                {
                    { "Foo&lt;T&gt;", "http://www.fooOfT.com" },
                    { "Foo", "http://www.foo.com" },
                };
                AutoLink autoLink = new AutoLink(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(expectedContent);
            }

            [Test]
            public void DoesNotRewriteOutsideQuerySelectorWhenNoReplacements()
            {
                // Given
                string inputContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                string expectedContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>();
                AutoLink autoLink = new AutoLink(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(expectedContent);
            }

            [Test]
            public void NoReplacementWithQuerySelectorReturnsSameDocument()
            {
                // Given
                string inputContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                string expectedContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>();
                AutoLink autoLink = new AutoLink(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                IList<IDocument> results = autoLink.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().ShouldBeSameAs(document);
            }
        }
    }
}