using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MirrorResourcesFixture : BaseFixture
    {
        public class ExecuteTests : MirrorResourcesFixture
        {
            [Test]
            public void ReplacesScriptResource()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                List<IDocument> result = module.Execute(new[] { document }, context).ToList();

                // Then
                result.Single().Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ReplacesLinkResource()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                List<IDocument> result = module.Execute(new[] { document }, context).ToList();

                // Then
                result.Single().Content.ShouldBe(
                    @"<html><head>
                        <link rel=""stylesheet"" href=""/mirror/cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"">
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotReplaceDataNoMirrorAttribute()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                List<IDocument> result = module.Execute(new[] { document }, context).ToList();

                // Then
                result.Single().Content.ShouldBe(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void UsesCustomMirrorPath()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources(x => new FilePath("/foo/bar.js"));

                // When
                List<IDocument> result = module.Execute(new[] { document }, context).ToList();

                // Then
                result.Single().Content.ShouldBe(
                    @"<html><head>
                        <script src=""/foo/bar.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
