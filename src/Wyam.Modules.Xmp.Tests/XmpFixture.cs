using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Modules.Xmp;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class XmpFixture : TraceListenerFixture
    {
        [Test]
        public void ReadMetadata()
        {
            // Given

            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out documents, out cloneDictionary, Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"));

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

            Xmp directoryMetadata = new Xmp()
                .WithMetadata("xmpRights:UsageTerms", "Copyright");
            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), Substitute.For<IExecutionContext>()).ToList();  // Make sure to materialize the result list

            // Then

            string expected = "This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.";

            Assert.True(cloneDictionary[documents[0]]
                .ContainsKey("Copyright"),
                "Metadata Copyright not found");
            Assert.AreEqual(expected, cloneDictionary[documents[0]]["Copyright"],
                "Metadata Copyright Wrong Value");
        }

        [Test]
        public void SkipMissingMandetory()
        {
            // Given

            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out documents, out cloneDictionary, 
                Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"), 
                Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\RomantiqueInitials.ttf"));

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

            Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: true)
                .WithMetadata("xmpRights:UsageTerms", "Copyright", true);
            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), Substitute.For<IExecutionContext>()).ToList();  // Make sure to materialize the result list

            // Then

            Assert.AreEqual(1, returnedDocuments.Count,
                "Wrong number of returned Documents");
        }

        [Test]
        public void DontSkipMissingMandetory()
        {
            // Given

            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out documents, out cloneDictionary, 
                Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"), 
                Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\RomantiqueInitials.ttf"));

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

            Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: false)
                .WithMetadata("xmpRights:UsageTerms", "Copyright", true);
            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), Substitute.For<IExecutionContext>()).ToList();  // Make sure to materialize the result list

            // Then

            Assert.AreEqual(2, returnedDocuments.Count,
                "Wrong number of returned Documents");
        }

        [Test]
        public void UsingNonDefaultNamespace()
        {
            // Given

            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out documents, out cloneDictionary, Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\Flamme.png"));

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");

            Xmp directoryMetadata = new Xmp()
                .WithNamespace("http://ns.adobe.com/xap/1.0/rights/", "bla")
                .WithMetadata("bla:UsageTerms", "Copyright");
            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents), Substitute.For<IExecutionContext>()).ToList();  // Make sure to materialize the result list

            // Then

            string expected = "This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.";

            Assert.True(cloneDictionary[documents[0]]
                .ContainsKey("Copyright"),
                "Metadata Copyright not found");
            Assert.AreEqual(expected, cloneDictionary[documents[0]]["Copyright"],
                "Metadata Copyright Wrong Value");
        }


        #region TestHelper

        private void Setup(out IDocument[] documents, out Dictionary<IDocument, IDictionary<string, object>> cloneDictionary, params string[] pathArray)
        {

            documents = pathArray.Select(x =>
            {
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(x);
                document.GetStream().Returns(File.OpenRead(x));

                return document;
            }).ToArray();

            var tempDictionary = new Dictionary<IDocument, IDictionary<string, object>>();
            cloneDictionary = tempDictionary;
            foreach (var document in documents)
            {
                //TODO: Change so muliple recursive calls to clone can be tracked.
                document
                    .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x =>
                    {
                        var newMetadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                        var oldMetadata = document.Metadata.ToDictionary(y => y.Key, y => y.Value);
                        foreach (var m in newMetadata) // overriding the old metadata like Document would do it.
                        {
                            oldMetadata[m.Key] = m.Value;
                        }
                        tempDictionary[document] = oldMetadata;
                    });
            }
        }





        #endregion
    }
}
