using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Sass.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SassFixture : BaseFixture
    {
        public class ExecuteTests : SassFixture
        {
            [Test]
            public void Convert()
            {
                string input = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = sass.Execute(new[] {document}, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] {output}));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public void ConvertingBadSassFails()
            {
                string input = @"
$font-stack:    Helvetica, sans-serif
$primary-color: #333

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass();

                // That
                Assert.Catch<AggregateException>(() =>
                {
                    sass.Execute(new[] {document}, context).ToList(); // Make sure to materialize the result list
                });
            }
        }
    }
}
