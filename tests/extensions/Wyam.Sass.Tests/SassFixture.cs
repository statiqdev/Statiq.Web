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
using Wyam.Testing.IO;

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
                // Given
                string input = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass().WithCompactOutputStyle();

                // When
                List<IDocument> results = sass.Execute(new[] {document}, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] {output}));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public void EmptyOutputForEmptyContent()
            {
                // Given
                string input = string.Empty;

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass();

                // When
                List<IDocument> results = sass.Execute(new[] { document }, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] { string.Empty }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public void ConvertingBadSassFails()
            {
                // Given
                string input = @"
$font-stack:    Helvetica, sans-serif
$primary-color: #333

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass();

                // When, Then
                Assert.Catch<AggregateException>(() =>
                {
                    sass.Execute(new[] {document}, context).ToList(); // Make sure to materialize the result list
                });
            }

            [Test]
            public void NestedImports()
            {
                // Given
                string outerImport = @"
$font-stack:    Helvetica, sans-serif;";
                string innerImport = @"
@import 'outer-import.scss';
$primary-color: #333;";
                string input = @"
@import 'libs/_inner-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_outer-import.scss", outerImport);
                fileProvider.AddFile("/input/assets/libs/_inner-import.scss", innerImport);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
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
            public void ImportWithoutExtension()
            {
                // Given
                string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                string input = @"
@import 'libs/_test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = sass.Execute(new[] { document }, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public void ImportWithoutPrefix()
            {
                // Given
                string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                string input = @"
@import 'libs/test-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = sass.Execute(new[] { document }, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            [Test]
            public void ImportWithoutPrefixOrExtension()
            {
                // Given
                string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                string input = @"
@import 'libs/test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                IExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                IDocument document = new TestDocument(input, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("assets/test.scss") }
                });

                Sass sass = new Sass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                List<IDocument> results = sass.Execute(new[] { document }, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EqualTo(new[] { output }));
                Assert.That(results.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath), Is.EqualTo(new[] { "assets/test.css" }));
            }

            // TODO: Change above test to just use exact file name
            // TODO: Test include with missing extension
            // TODO: Test include with _ prefix
            // TODO: Test include with missing extension and _ prefix
        }
    }
}
