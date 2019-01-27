using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Images.Tests
{
    [TestFixture]
    public class ImagesFixture : BaseFixture
    {
        public class ExecuteTests : ImagesFixture
        {
            [Test]
            public void OutputsTheSameAsInput()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(fileStream, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("a/b/test.png") }
                });
                Image module = new Image();

                // When
                IList<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Count.ShouldBe(1);
                results[0].Get<FilePath>(Keys.WritePath).FullPath.ShouldBe("/output/a/b/test.png");
            }

            [Test]
            public void ChangesPathWhenOutputFormatSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(fileStream, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("a/b/test.png") }
                });
                Image module = new Image().OutputAsGif();

                // When
                IList<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Count.ShouldBe(1);
                results[0].Get<FilePath>(Keys.WritePath).FullPath.ShouldBe("/output/a/b/test.gif");
            }

            [Test]
            public void ChangesPathWhenBrightnessSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(fileStream, new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("a/b/test.png") }
                });
                Image module = new Image().Brightness(123);

                // When
                IList<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Count.ShouldBe(1);
                results[0].Get<FilePath>(Keys.WritePath).FullPath.ShouldBe("/output/a/b/test-b123.png");
            }
        }
    }
}
