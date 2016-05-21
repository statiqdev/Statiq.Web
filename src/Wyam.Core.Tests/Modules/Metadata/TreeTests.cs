using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class TreeTests : BaseFixture
    {
        public class ExecuteMethodTests : TreeTests
        {
            [Test]
            public void TestNumberOfInputiEquealsOutput()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    @"\root",
                    @"\root\a",
                    @"\root\a\1",
                    @"\root\a\2",
                    @"\root\b",
                    @"\root\b\1",
                    @"\root\c",
                    @"\root\c\2",
                    @"\root\d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual(inputPaths.Length, outputDocuments.Count);
            }

            [Test]
            public void TestAddingMissingNodes()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    @"\root",
                    @"\root\a\1",
                    @"\root\a\2",
                    @"\root\b\1",
                    @"\root\c\2",
                    @"\root\d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list
                
                // Then
                Assert.AreEqual(inputPaths.Length + 3, outputDocuments.Count);
            }

            [Test]
            public void TestCorrectParent()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());
                const string metadatakey = "Parent";
                Assert.AreEqual(null,
                    outputLookup[@"/root"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root"],
                    outputLookup[@"/root/a"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/a/1"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/a/2"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root"],
                    outputLookup[@"/root/b"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/b/1"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root"],
                    outputLookup[@"/root/c"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/c/2"][metadatakey]);
                Assert.AreEqual(outputLookup[@"/root"],
                    outputLookup[@"/root/d"][metadatakey]);
            }

            [Test]
            public void TestAditionalRoots()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree().WithRoots((doc, config) => doc.Source.Directory.Name == "root");

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());

                Assert.AreEqual(0,
                    ((IReadOnlyCollection<IDocument>) outputLookup[@"/root"]["Children"]).Count);


                const string metadatakey = "Parent";
                Assert.AreEqual(null,
                    outputLookup[@"/root"][metadatakey]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/a"][metadatakey]);

                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/a/1"][metadatakey]);

                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/a/2"][metadatakey]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/b"][metadatakey]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/b/1"][metadatakey]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/c"][metadatakey]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/c/2"][metadatakey]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/d"][metadatakey]);
            }

            [Test]
            public void TestCustomOrder()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree().WithOrder((doc, config) =>
                {
                    switch (doc.Source.FileName.ToString())
                    {
                        case "a":
                            return 4;
                        case "b":
                            return 3;
                        case "c":
                            return 2;
                        case "d":
                            return 1;
                        default:
                            return -1;
                    }
                });

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());

                // Then
                Assert.AreEqual(outputLookup[@"/root/d"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/a"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);
            }

            [Test]
            public void TestDefaultOrder()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());

                // Then
                Assert.AreEqual(outputLookup[@"/root/a"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);

                Assert.AreEqual(outputLookup[@"/root/d"],
                    ((IReadOnlyList<IDocument>) outputLookup[@"/root"]["Children"])[0]);
            }

            [Test]
            public void TestCorrectNext()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());
                const string metadataToLookup = "Next";
                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/1"],
                    outputLookup[@"/root/a"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/2"],
                    outputLookup[@"/root/a/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/a/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b/1"],
                    outputLookup[@"/root/b"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/b/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c/2"],
                    outputLookup[@"/root/c"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/d"],
                    outputLookup[@"/root/c/2"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/d"][metadataToLookup]);
            }

            [Test]
            public void TestCorrectPreview()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());
                const string metadataToLookup = "Previous";
                Assert.AreEqual(null,
                    outputLookup[@"/root"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root"],
                    outputLookup[@"/root/a"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/a/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/1"],
                    outputLookup[@"/root/a/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/2"],
                    outputLookup[@"/root/b"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/b/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b/1"],
                    outputLookup[@"/root/c"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/c/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c/2"],
                    outputLookup[@"/root/d"][metadataToLookup]);
            }

            [Test]
            public void TestCorrectPreviousSibling()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());
                const string metadataToLookup = "PreviousSibling";
                Assert.AreEqual(null,
                    outputLookup[@"/root"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/a"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/a/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/1"],
                    outputLookup[@"/root/a/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a"],
                    outputLookup[@"/root/b"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/b/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/c"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/c/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/d"][metadataToLookup]);
            }

            [Test]
            public void TestCorrectNextSibling()
            {
                // Given
                IExecutionContext context;
                IDictionary<string, IDocument> documents;
                IDictionary<IDocument, int> documentsIndex;
                Dictionary<IDocument, IDictionary<string, object>> cloneDictionary;
                String[] inputPaths = new String[]
                {
                    "/root",
                    "/root/a",
                    "/root/a/1",
                    "/root/a/2",
                    "/root/b",
                    "/root/b/1",
                    "/root/c",
                    "/root/c/2",
                    "/root/d"
                };
                Setup(out context, out documents, out documentsIndex, out cloneDictionary, inputPaths);

                Tree tree = new Tree();

                // When
                List<IDocument> outputDocuments = tree.Execute(new List<IDocument>(documents.Values), context).ToList();  // Make sure to materialize the result list

                // Then
                Dictionary<string, IDocument> outputLookup = outputDocuments.ToDictionary(x => x.Source.FullPath.ToString());
                const string metadataToLookup = "NextSibling";
                Assert.AreEqual(null,
                    outputLookup[@"/root"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/b"],
                    outputLookup[@"/root/a"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/a/2"],
                    outputLookup[@"/root/a/1"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/a/2"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/c"],
                    outputLookup[@"/root/b"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/b/1"][metadataToLookup]);

                Assert.AreEqual(outputLookup[@"/root/d"],
                    outputLookup[@"/root/c"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/c/2"][metadataToLookup]);

                Assert.AreEqual(null,
                    outputLookup[@"/root/d"][metadataToLookup]);
            }
        }

        #region TestHelper

        private const string Root = @"c:\wyam\";
        private const string SubLocal = @"1\local.metadata";
        private const string SubInherited = @"1\inherit.metadata";
        private const string Local = @"local.metadata";
        private const string Inherited = @"inherit.metadata";
        private const string SubSite = @"1\site.md";
        private const string Site = @"site.md";

        /// <summary>
        /// This method helps to create many stubs that are used for all tests.
        /// </summary>
        /// <param name="context">This is the context that can be used by the test.</param>
        /// <param name="documents">A dictionary with (relativePath, document)</param>
        /// <param name="documentsIndexLookup">A directory that has maps an index to every document.</param>
        /// <param name="cloneDictionary">For every Document that was cloned the new Metadata is stored.</param>
        /// <param name="pathArray">
        ///     The paths for which documents will be generated.
        ///     If empty, 6 default documents will be generated.
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
                    Site,
                    Local,
                    Inherited,
                    SubSite,
                    SubLocal,
                    SubInherited
               };
            }

            var documentsArray = pathArray.Select((path, index) => new
            {
                Index = index,
                Document = GetDocumentWithMetadata(path, GenerateMetadata(index, pathArray.Length).ToArray()),
                Path = path
            }).ToArray();

            documents = documentsArray.ToDictionary(x => x.Path, x => x.Document);
            documentsIndexLookup = documentsArray.ToDictionary(x => x.Document, x => x.Index);
            Dictionary<IDocument, IDictionary<string, object>> tempDictionary = new Dictionary<IDocument, IDictionary<string, object>>();
            cloneDictionary = tempDictionary;
            context.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x =>
                {
                    IDocument document = x.Arg<IDocument>();
                    IEnumerable<KeyValuePair<string, object>> newMetadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();

                    IDocument newDocument = GetDocumentWithMetadata(document?.Source?.ToString(), newMetadata?.ToArray() ?? new KeyValuePair<string, object>[0]);

                    foreach (KeyValuePair<string, object> m in newMetadata) // overriding the old metadata like Document would do it.
                    {
                        newDocument.Get<IDocument>(m.Key).Returns(m.Value as IDocument);
                        newDocument.Get<IReadOnlyList<IDocument>>(m.Key).Returns(m.Value as IReadOnlyList<IDocument>);
                    }

                    Dictionary<string, object> oldMetadata = document.Metadata.ToDictionary(y => y.Key, y => y.Value);
                    foreach (KeyValuePair<string, object> m in newMetadata) // overriding the old metadata like Document would do it.
                    {
                        oldMetadata[m.Key] = m.Value;
                    }
                    tempDictionary[document] = oldMetadata;



                    return newDocument;
                });
        }

        /// <summary>
        /// Returns the Key of the Metadata that is exactly set in all submitted documents.
        /// </summary>
        /// <param name="whereHas">The Documents represented by their index.</param>
        /// <returns>The keyword that is only available on the required documents.</returns>
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
                {
                    yield return $"m{i}";
                }
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
            yield break;
        }

        private static IExecutionContext GetContext()
        {
            IExecutionContext context = Substitute.For<IExecutionContext>();
            AddTryConvert<bool>(context);
            DirectoryPath inputPath = new DirectoryPath(Root);
            context.FileSystem.InputPaths.Returns(new[] { inputPath });
            context.FileSystem.GetContainingInputPath(Arg.Any<NormalizedPath>()).Returns(inputPath);
            return context;
        }

        /// <summary>
        /// Adds convert functionality to the ExecutionContext.
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
        private IDocument GetDocumentWithMetadata(string relativePath, KeyValuePair<string, object>[] metadata)
        {

            for (int i = 0; i < metadata.Length; i++)
            {
                IMetadataValue provider = metadata[i].Value as IMetadataValue;
                if (provider != null)
                    metadata[i] = new KeyValuePair<string, object>(metadata[i].Key, provider.Get(metadata[i].Key, null));
            }

            IDocument document = Substitute.For<IDocument>();
            IMetadata metadataObject = Substitute.For<IMetadata>();

            metadataObject.GetEnumerator().Returns(x => metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());
            metadataObject.Keys.Returns(metadata.Select(x => x.Key));
            metadataObject.Values.Returns(metadata.Select(x => x.Value));

            document.Source.Returns(relativePath);
            document.Metadata.Returns(metadataObject);
            document.GetEnumerator().Returns(x => metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());
            document.Keys.Returns(metadata.Select(x => x.Key));
            document.Values.Returns(metadata.Select(x => x.Value));

            foreach (KeyValuePair<string, object> m in metadata)
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
