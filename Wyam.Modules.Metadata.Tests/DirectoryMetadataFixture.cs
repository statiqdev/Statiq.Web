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
        private const string ROOT = @"c:\wyam\";



        [Test]
        public void MetadataObjectsAreFilterd()
        {
            // Given

            IExecutionContext context;
            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(documents, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(2, returnedDocuments.Count);
        }


        [Test]
        public void MetadataObjectsAreNotFilterdIfPreserved()
        {
            // Given

            IExecutionContext context;
            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata().WithPreserveMetadataFiles();

            // When
            var returnedDocuments = directoryMetadata.Execute(documents, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(6, returnedDocuments.Count);
        }


        [Test]
        public void TestIfLocal()
        {
            // Given

            IExecutionContext context;
            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata().WithPreserveMetadataFiles();

            // When
            var returnedDocuments = directoryMetadata.Execute(documents, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[4]].ContainsKey(ListMetadataWhereSingel(0)), "Data from local not found");
            Assert.True(cloneDictionary[documents[5]].ContainsKey(ListMetadataWhereSingel(2)), "Data from local not found");
            Assert.True(!cloneDictionary[documents[4]].ContainsKey(ListMetadataWhereSingel(2)), "Data from local one directory obove found.");
            Assert.AreEqual(6, returnedDocuments.Count);
        }

        [Test]
        public void TestInhired()
        {
            // Given

            IExecutionContext context;
            IDocument[] documents;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out cloneDictionary);

            DirectoryMetadata directoryMetadata = new DirectoryMetadata().WithPreserveMetadataFiles();

            // When
            var returnedDocuments = directoryMetadata.Execute(documents, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[4]].ContainsKey(ListMetadataWhereSingel(1)), "Data from inhired on same level not found");
            Assert.True(cloneDictionary[documents[5]].ContainsKey(ListMetadataWhereSingel(3)), "Data from inhired not found on same level");
            Assert.True(cloneDictionary[documents[4]].ContainsKey(ListMetadataWhereSingel(3)), "Data from inhired not found from level abouve");
            Assert.False(cloneDictionary[documents[5]].ContainsKey(ListMetadataWhereSingel(1)), "Data from inhired on level below found");
        }




        #region TestHelper

        private void Setup(out IExecutionContext context, out IDocument[] documents, out Dictionary<IDocument, IDictionary<string, object>> cloneDictionary)
        {
            context = GetContext();
            documents = new IDocument[] {
                GetDocumentWithMetadata(@"test\1\local.metadata",
                    GenerateMetadata(0,6)),
                GetDocumentWithMetadata(@"test\1\inherit.metadata",
                    GenerateMetadata(1,6)),
                GetDocumentWithMetadata(@"test\local.metadata",
                    GenerateMetadata(2,6)),
                GetDocumentWithMetadata(@"test\inherit.metadata",
                    GenerateMetadata(3,6)),
                GetDocumentWithMetadata(@"test\1\site.md",
                    GenerateMetadata(4,6)),
                GetDocumentWithMetadata(@"test\site.md",
                    GenerateMetadata(5,6))
            };
            var tempDictionary = new Dictionary<IDocument, IDictionary<string, object>>();
            cloneDictionary = tempDictionary;
            foreach (var document in documents)
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


        private IEnumerable<string> ListAllMetadata(int max, IEnumerable<int> whereHas = null, IEnumerable<int> whereDoesnt = null)
        {
            whereHas = whereHas ?? Enumerable.Empty<int>();
            whereDoesnt = whereDoesnt ?? Enumerable.Empty<int>();
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
