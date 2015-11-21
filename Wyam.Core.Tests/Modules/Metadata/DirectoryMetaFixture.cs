using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules.Metadata;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class DirectoryMetaFixture
    {
        [Test]
        public void MetadataObjectsAreFiltered()
        {
            // Given

            IExecutionContext context;
            IDictionary<string, IDocument> documents;
            IDictionary<IDocument, int> documentsIndex;
            Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
            Setup(out context, out documents, out documentsIndex, out cloneDictionary);

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL)
                .WithMetadataFile(INHERITED, true);

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

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL)
                .WithMetadataFile(INHERITED, true)
                .WithPreserveMetadataFiles();

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

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL)
                .WithMetadataFile(INHERITED, true);

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[LOCAL]])),
                "Data from local not found");
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[SUB_LOCAL]])),
                "Data from local not found");
            Assert.False(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[LOCAL]])),
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

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL)
                .WithMetadataFile(INHERITED, true);

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[SUB_INHERITED]])),
                "Data from inhired on same level not found");
            Assert.True(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[INHERITED]])),
                "Data from inhired not found on same level");
            Assert.True(cloneDictionary[documents[SUB_SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[INHERITED]])),
                "Data from inhired not found from level abouve");
            Assert.False(cloneDictionary[documents[SITE]]
                .ContainsKey(ListMetadata(documentsIndex[documents[SUB_INHERITED]])),
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

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL)
                .WithMetadataFile(INHERITED, true);

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then

            // if Metadata is in all take from original file
            Assert.AreEqual(documentsIndex[documents[SUB_SITE]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHERITED]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {SUB_SITE}.");

            // if File metadata is Missinge it must be from sub_local
            Assert.AreEqual(documentsIndex[documents[SUB_LOCAL]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHERITED]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {SUB_LOCAL}.");

            // if sub_local also missing it is from sub_inhired
            Assert.AreEqual(documentsIndex[documents[SUB_INHERITED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_INHERITED]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {SUB_INHERITED}.");
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

            DirectoryMeta directoryMetadata = new DirectoryMeta()
                .WithMetadataFile(LOCAL, replace: true)
                .WithMetadataFile(INHERITED, true, true);

            // When
            var returnedDocuments = directoryMetadata.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

            // Then

            // if Metadata is in all take from sub_Local
            Assert.AreEqual(documentsIndex[documents[SUB_LOCAL]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_LOCAL]],
                        documentsIndex[documents[SUB_INHERITED]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {SUB_LOCAL}.");

            // if sub_local is missing it is from sub_inhired
            Assert.AreEqual(documentsIndex[documents[SUB_INHERITED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[SUB_INHERITED]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {SUB_INHERITED}.");

            // if sub_inhired is missing it is from inhired
            Assert.AreEqual(documentsIndex[documents[INHERITED]],
                cloneDictionary[documents[SUB_SITE]]
                    [ListMetadata(documentsIndex[documents[SUB_SITE]],
                        documentsIndex[documents[INHERITED]]
                    )], $"Metadata should be the one of the {INHERITED}.");
        }

        #region TestHelper

        private const string ROOT = @"c:\wyam\";
        private const string SUB_LOCAL = @"1\local.metadata";
        private const string SUB_INHERITED = @"1\inherit.metadata";
        private const string LOCAL = @"local.metadata";
        private const string INHERITED = @"inherit.metadata";
        private const string SUB_SITE = @"1\site.md";
        private const string SITE = @"site.md";

        /// <summary>
        /// This Method Helps to Create many Stubs that are used for all tests.
        /// </summary>
        /// <param name="context">This is the context that can be used by the test.</param>
        /// <param name="documents">A dictionary with (relativePath, document)</param>
        /// <param name="documentsIndexLookup">A directory that has maps an index to every document.</param>
        /// <param name="cloneDictionary">For every Document that was cloned the new Metadata is stored.</param>
        /// <param name="pathArray">
        ///     The Pathes for wich documents will be generated.
        ///     If empty 6 default documents will be genreated.
        /// </param>
        /// <remarks>
        /// Each document will be generated with metadata.
        /// 
        /// The Value of the Metadata is always
        /// the index that would be returned by the <paramref name="documentsIndexLookup"/>
        /// for this Document.
        /// 
        /// The Key is a string that starts with m and is followed by a number. The total
        /// number of all different keywords is 2^Number of Documents. And an Document has metadata for all
        /// Keywords where where the binary representation of the number followed by the m has the nth bit set.
        /// For every document n is its documentsIndex. 
        /// </remarks>
        private void Setup(out IExecutionContext context, out IDictionary<string, IDocument> documents, out IDictionary<IDocument, int> documentsIndexLookup, out Dictionary<IDocument, IDictionary<string, object>> cloneDictionary, params string[] pathArray)
        {
            context = GetContext();
            if (pathArray.Length == 0)
            {
                pathArray = new string[]
                {
                    SITE,
                    LOCAL,
                    INHERITED,
                    SUB_SITE,
                    SUB_LOCAL,
                    SUB_INHERITED
               };
            }

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
                //TODO: Change so multiple recursive calls to clone can be tracked.
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

        /// <summary>
        /// Returns the Key of the Metadata that is exactly set in all submitted documents.
        /// </summary>
        /// <param name="whereHas">The Documents represented by ther index.</param>
        /// <returns>The keyword that is only available on the reqired documents.</returns>
        private string ListMetadata(params int[] whereHas)
        {
            return ListAllMetadata(whereHas.Max() + 1, whereHas as IEnumerable<int>).Single();
        }

        /// <summary>
        /// Returns all Keywords that are present in <paramref name="whereHas"/> but
        /// not in <paramref name="whereDoesnt"/>.
        /// </summary>
        /// <param name="numberOfDocuments">The total number of documents.</param>
        /// <param name="whereHas">The Documents that will have the keywords. represented by ther index.</param>
        /// <param name="whereDoesnt">The Documents that won't have the keywords. represented by ther index.</param>
        /// <returns>The List of Keywords</returns>
        private IEnumerable<string> ListAllMetadata(int numberOfDocuments, IEnumerable<int> whereHas, IEnumerable<int> whereDoesnt = null)
        {
            whereHas = whereHas ?? Enumerable.Empty<int>();
            whereDoesnt = whereDoesnt ?? Enumerable.Range(0, numberOfDocuments).Except(whereHas);
            Func<int, int, bool> check = (index, i) =>
            {
                int currentPotenz = (int)Math.Pow(2, index);
                return (i & currentPotenz) == currentPotenz;

            };
            for (int i = 0; i < Math.Pow(2, numberOfDocuments); i++)
            {
                if (whereHas.All(index => check(index, i)) && whereDoesnt.All(index => !check(index, i)))
                    yield return $"m{i}";
            }
        }

        /// <summary>
        /// Generates Metadata for a Document with a Specific index.
        /// </summary>
        /// <param name="index">The index of the document.</param>
        /// <param name="numberOfDocuments">The total amount of Documents.</param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string, object>> GenerateMetadata(int index, int numberOfDocuments)
        {
            int currentPotenz = (int)Math.Pow(2, index);
            for (int i = 0; i < Math.Pow(2, numberOfDocuments) - 1; i++)
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

        /// <summary>
        /// Add's convert functionalaty to the ExecutionContext.
        /// </summary>
        /// <typeparam name="T">The Type that should be converted.</typeparam>
        /// <param name="context">The Context that will convert.</param>
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

        /// <summary>
        /// Creates a stub of a Document.
        /// </summary>
        /// <param name="relativePath">The Path of the Document.</param>
        /// <param name="metadata">The Metadata of the Document.</param>
        /// <returns></returns>
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
