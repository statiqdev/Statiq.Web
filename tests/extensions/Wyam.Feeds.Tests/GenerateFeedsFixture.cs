using System;
using System.Collections.Generic;
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

namespace Wyam.Feeds.Tests
{
    [TestFixture]
    public class GenerateFeedsFixture : BaseFixture
    {
        public class ExecuteTests : GenerateFeedsFixture
        {
            [Test]
            public void DoesNotChangeImageDomain()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.AddTypeConversion<string, Uri>(x => new Uri(x));
                context.Settings[Keys.Host] = "buzz.com";
                TestDocument document = new TestDocument(new Dictionary<string, object>
                {
                    { Keys.RelativeFilePath, new FilePath("fizz/buzz") },
                    { FeedKeys.Image, new Uri("http://foo.com/bar/baz.png") }
                });
                document.AddTypeConversion<Uri, string>(x => x.ToString());
                document.AddTypeConversion<FilePath, string>(x => x.FullPath);
                GenerateFeeds module = new GenerateFeeds();

                // When
                IList<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.FilePath(Keys.WritePath).FullPath).ShouldBe(new[] { "feed.rss", "feed.atom" }, true);
                results[0].Content.ShouldContain("http://foo.com/bar/baz.png");
            }
        }
    }
}
