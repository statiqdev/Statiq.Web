using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Modules.Metadata;

namespace Wyam.Modules.Html.Tests
{
    [TestFixture]
    public class AutoLinkFixture
    {
        [Test]
        public void MetadataObjectsAreFilterd()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(2, returnedDocuments.Count);
        }

        [Test]
        public void MetadataObjectsAreNotFilterdIfPreserved()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata().WithPreserveMetadataFiles();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(6, returnedDocuments.Count);
        }

        [Test]
        public void TestIfLocal()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[LOCAL]])),
                "Data from local not found");
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[SUB_LOCAL]])),
                "Data from local not found");
            Assert.False(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[LOCAL]])),
                "Data from local one directory obove found.");
        }

        [Test]
        public void TestInhired()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[SUB_INHIRED]])),
                "Data from inhired on same level not found");
            Assert.True(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[INHIRED]])),
                "Data from inhired not found on same level");
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[INHIRED]])),
                "Data from inhired not found from level abouve");
            Assert.False(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadataWhereSingel(documentsIndex[documents[SUB_INHIRED]])),
                "Data from inhired on level below found");
        }

        [Test]
        public void TestOverrideBehavior()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then

            // if Metadata is in all take from original file
            Assert.AreEqual(documentsIndex[documents[SUB_SITE]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHIRED]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {SUB_SITE}.");

            // if File metadata is Missinge it must be from sub_local
            Assert.AreEqual(documentsIndex[documents[SUB_LOCAL]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHIRED]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {SUB_LOCAL}.");

            // if sub_local also missing it is from sub_inhired
            Assert.AreEqual(documentsIndex[documents[SUB_INHIRED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_INHIRED]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {SUB_INHIRED}.");
        }

        [Test]
        public void TestOverrideBehaviorWithOverideEnabled()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata().WithOverride();

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then

            // if Metadata is in all take from sub_Local
            Assert.AreEqual(documentsIndex[documents[SUB_LOCAL]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHIRED]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {SUB_LOCAL}.");

            // if sub_local is missing it is from sub_inhired
            Assert.AreEqual(documentsIndex[documents[SUB_INHIRED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_INHIRED]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {SUB_INHIRED}.");

            // if sub_inhired is missing it is from inhired
            Assert.AreEqual(documentsIndex[documents[INHIRED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[INHIRED]]
                    )], $"Metadata should be the one of the {INHIRED}.");
        }

        #region TestHelper

        private const string ROOT = @"c:\wyam\";
        private const string SUB_LOCAL = @"1\local.metadata";
        private const string SUB_INHIRED = @"1\inherit.metadata";
        private const string LOCAL = @"local.metadata";
        private const string INHIRED = @"inherit.metadata";
        private const string SUB_SITE = @"1\site.md";
        private const string SITE = @"site.md";


        private void Setup(out IExecutionContext context, out IDictionary<string, IDocument> documents, out IDictionary<IDocument, int> documentsIndexLookup, out Dictionary<IDocument, IDictionary<string, object>> cloneDictionary)
        {
            context = GetContext();
            string[] pathArray =
            {
                SITE,
                LOCAL,
                INHIRED,
                SUB_SITE,
                SUB_LOCAL,
                SUB_INHIRED
            };

            var documentsArray = pathArray.Select((path, index) => new
            {
                Index = index,
                Document = GetDocumentWithMetadata(path, GenerateMetadata(index, pathArray.Length)),
                Path = path
            }).ToArray();

            documents = documentsArray.ToDictionary(x => x.Path, x => x.Document);
            documentsIndexLookup = documentsArray.ToDictionary(x => x.Document, x => x.Index);
            var tempDictionary = new Dictionary<IDocument, IDictionary<string, object>>();
            cloneDictionary = tempDictionary;
            foreach (var document in documents.Values)
            {
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

        private string ListMetadata(params int[] whereHas)
        {
            return ListAllMetadata(whereHas.Max()+1, whereHas as IEnumerable<int>).Single();
        }

        private IEnumerable<string> ListAllMetadata(int max, IEnumerable<int> whereHas, IEnumerable<int> whereDoesnt = null)
        {
            whereHas = whereHas ?? Enumerable.Empty<int>();
            whereDoesnt = whereDoesnt ?? Enumerable.Range(0, max).Except(whereHas);
            Func<int, int, bool> check = (index, i) =>
            {
                int currentPotenz = (int)Math.Pow(2, index);
                return (i & currentPotenz) == currentPotenz;

            };
            for (int i = 0; i < Math.Pow(2, max); i++)
            {
                if (whereHas.All(index => check(index, i)) && whereDoesnt.All(index => !check(index, i)))
                    yield return $"m{i}";
            }
        }

        private string ListMetadataWhereSingel(int singel)
        {
            return $"m{(int)Math.Pow(2, singel)}";
        }
        private IEnumerable<KeyValuePair<string, object>> GenerateMetadata(int index, int max)
        {
            int currentPotenz = (int)Math.Pow(2, index);
            for (int i = 0; i < Math.Pow(2, max) - 1; i++)
            {
                if ((i & currentPotenz) == currentPotenz)
                    yield return new KeyValuePair<string, object>($"m{i}", index);
            }
        }

        private static IExecutionContext GetContext()
        {
            IExecutionContext context = Substitute.For<IExecutionContext>();
            AddTryConvert<bool>(context);
            context.InputFolder.Returns(ROOT);
            return context;
        }

        private static void AddTryConvert<T>(IExecutionContext context)
        {
            T anyBool;
            context.TryConvert(Arg.Any<object>(), out anyBool).ReturnsForAnyArgs(x =>
            {
                try
                {
                    x[1] = (T)x[0];

                }
                catch (InvalidCastException)
                {
                    return false;
                }
                return true;
            });
        }

        private IDocument GetDocumentWithMetadata(string relativePath, IEnumerable<KeyValuePair<string, object>> metadata)
        {
            IDocument document = Substitute.For<IDocument>();
            IMetadata metadataObject = Substitute.For<IMetadata>();

            metadataObject.GetEnumerator().Returns(x => metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());
            metadataObject.Keys.Returns(metadata.Select(x => x.Key));
            metadataObject.Values.Returns(metadata.Select(x => x.Value));

            document.Source.Returns(Path.Combine(ROOT, "input", relativePath));
            document.Metadata.Returns(metadataObject);
            document.GetEnumerator().Returns(x => metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());
            document.Keys.Returns(metadata.Select(x => x.Key));
            document.Values.Returns(metadata.Select(x => x.Value));

            foreach (var m in metadata)
            {
                metadataObject[m.Key].Returns(m.Value);
                metadataObject.ContainsKey(Arg.Any<string>()).Returns((x => metadata.Any(y => y.Key == (string)x[0])));

                document[m.Key].Returns(m.Value);
                document.ContainsKey(Arg.Any<string>()).Returns((x => metadata.Any(y => y.Key == (string)x[0])));
            }

            return document;
        }

        #endregion
    }
}
