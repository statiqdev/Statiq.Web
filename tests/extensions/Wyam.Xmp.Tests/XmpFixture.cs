using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Xmp.Tests
{
    [TestFixture]
    public class XmpFixture : BaseFixture
    {
        public class ExecuteTests : XmpFixture
        {
            [Test]
            public void ReadMetadata()
            {
                // Given
                IExecutionContext context;
                IDocument[] documents;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                Setup(out context, out documents, out cloneDictionary, Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"));

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

                Xmp directoryMetadata = new Xmp()
                    .WithMetadata("xmpRights:UsageTerms", "Copyright");

                // When
                List<IDocument> returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then

                string expected = "This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.";

                Assert.True(
                    cloneDictionary[documents[0]]
                    .ContainsKey("Copyright"),
                    "Metadata Copyright not found");
                Assert.AreEqual(
                    expected,
                    cloneDictionary[documents[0]]["Copyright"],
                    "Metadata Copyright Wrong Value");
            }

            [Test]
            public void SkipMissingMandatory()
            {
                // Given
                ThrowOnTraceEventType(TraceEventType.Error);
                IDocument[] documents;
                IExecutionContext context;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                Setup(
                    out context,
                    out documents,
                    out cloneDictionary,
                    Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\RomantiqueInitials.ttf"));

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: true)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                List<IDocument> returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual(
                    1,
                    returnedDocuments.Count,
                    "Wrong number of returned Documents");
            }

            [Test]
            public void DontSkipMissingMandatory()
            {
                // Given
                ThrowOnTraceEventType(TraceEventType.Error);
                IDocument[] documents;
                IExecutionContext context;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                Setup(
                    out context,
                    out documents,
                    out cloneDictionary,
                    Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\RomantiqueInitials.ttf"));

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: false)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                List<IDocument> returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual(
                    2,
                    returnedDocuments.Count,
                    "Wrong number of returned Documents");
            }

            [Test]
            public void UsingNonDefaultNamespace()
            {
                // Given
                IExecutionContext context;
                IDocument[] documents;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                Setup(out context, out documents, out cloneDictionary, Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"));

                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

                Xmp directoryMetadata = new Xmp()
                    .WithNamespace("http://ns.adobe.com/xap/1.0/rights/", "bla")
                    .WithMetadata("bla:UsageTerms", "Copyright");

                // When
                List<IDocument> returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                string expected = "This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.";

                Assert.True(
                    cloneDictionary[documents[0]]
                    .ContainsKey("Copyright"),
                    "Metadata Copyright not found");
                Assert.AreEqual(
                    expected,
                    cloneDictionary[documents[0]]["Copyright"],
                    "Metadata Copyright Wrong Value");
            }

            private void Setup(out IExecutionContext context, out IDocument[] documents, out Dictionary<IDocument, IDictionary<string, object>> cloneDictionary, params string[] pathArray)
            {
                documents = pathArray.Select(x =>
                {
                    IDocument document = Substitute.For<IDocument>();
                    document.Source.Returns(x);
                    document.GetStream().Returns(File.OpenRead(x));

                    return document;
                }).ToArray();

                Dictionary<IDocument, IDictionary<string, object>> tempDictionary = new Dictionary<IDocument, IDictionary<string, object>>();
                cloneDictionary = tempDictionary;
                context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x =>
                    {
                        IDocument document = x.Arg<IDocument>();
                        IEnumerable<KeyValuePair<string, object>> newMetadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                        Dictionary<string, object> oldMetadata = document.Metadata.ToDictionary(y => y.Key, y => y.Value);
                        foreach (KeyValuePair<string, object> m in newMetadata)
                        {
                            oldMetadata[m.Key] = m.Value;
                        }
                        tempDictionary[document] = oldMetadata;
                    });
            }
        }
    }
}