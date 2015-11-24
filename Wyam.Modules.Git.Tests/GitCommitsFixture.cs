using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Git.Tests
{
    [TestFixture]
    public class GitCommitsFixture
    {
        [Test]
        public void GetAllCommitsFromInputPath()
        {
            // Given
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.InputFolder.Returns(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)))); // Ascend from \bin\Debug
            context.GetNewDocument(Arg.Any<IEnumerable<MetadataItem>>()).Returns(getNewDocumentCallInfo =>
            {
                IDocument newDocument = Substitute.For<IDocument>();
                newDocument.GetEnumerator()
                    .Returns(getNewDocumentCallInfo.ArgAt<IEnumerable<MetadataItem>>(0).Select(x => (KeyValuePair<string, object>)x).GetEnumerator());
                return newDocument;
            });
            IDocument document = Substitute.For<IDocument>();
            GitCommits gitCommits = new GitCommits();

            // When
            List<IDocument> results = gitCommits.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.That(results.Count, Is.GreaterThan(400));
            Assert.AreEqual("cbd6fb2b7e7fc6e0bbe19744433a510cef93bd7f", results.Last().First(x => x.Key == GitKeys.Sha).Value);
        }

        [Test]
        public void GetCommitsForInputDocument()
        {
            // Given
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.InputFolder.Returns(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)))); // Ascend from \bin\Debug
            context.GetNewDocument(Arg.Any<IEnumerable<MetadataItem>>()).Returns(getNewDocumentCallInfo =>
            {
                IDocument newDocument = Substitute.For<IDocument>();
                newDocument.GetEnumerator()
                    .Returns(getNewDocumentCallInfo.ArgAt<IEnumerable<MetadataItem>>(0).Select(x => (KeyValuePair<string, object>)x).GetEnumerator());
                newDocument.Get<IReadOnlyDictionary<string, string>>(Arg.Any<string>())
                    .Returns(getCallInfo => (IReadOnlyDictionary<string, string>)getNewDocumentCallInfo.ArgAt<IEnumerable<MetadataItem>>(0).First(x => x.Key == getCallInfo.ArgAt<string>(0)).Value);
                return newDocument;
            });
            IDocument document = Substitute.For<IDocument>();
            document.Source.Returns(
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory))),
                    "Wyam.Core\\IModule.cs"));  // Use file that no longer exists so commit count is stable
            document.Clone(Arg.Any<IEnumerable<MetadataItem>>()).Returns(x =>
            {
                IDocument newDocument = Substitute.For<IDocument>();
                newDocument.GetEnumerator().Returns(x.ArgAt<IEnumerable<MetadataItem>>(0).Select(y => (KeyValuePair<string, object>)y).GetEnumerator());
                return newDocument;
            });
            GitCommits gitCommits = new GitCommits().ForEachInputDocument();

            // When
            List<IDocument> results = gitCommits.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(1, results.Count);
            List<IDocument> commits =
                ((IEnumerable<IDocument>) results[0].First(x => x.Key == GitKeys.Commits).Value).ToList();
            Assert.AreEqual(6, commits.Count);
            Assert.AreEqual("6274fb76a0380760ab2dc83f90748b7d953aceb4", commits.Last().First(x => x.Key == GitKeys.Sha).Value);
        }
    }
}
