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

            IExecutionContext context = GetContext();

            IDocument[] documents = {
                GetDocumentWithMetadata(@"test\1\loca.metadata",
                    new KeyValuePair<string, object>("m1",1)),
                GetDocumentWithMetadata(@"test\1\inhired.metadata",
                    new KeyValuePair<string, object>("m1",1)),
                GetDocumentWithMetadata(@"test\loca.metadata",
                    new KeyValuePair<string, object>("m1",1)),
                GetDocumentWithMetadata(@"test\inhired.metadata",
                    new KeyValuePair<string, object>("m1",1)),
                GetDocumentWithMetadata(@"test\1\site.md",
                    new KeyValuePair<string, object>("m1",1)),
                GetDocumentWithMetadata(@"test\site.md",
                    new KeyValuePair<string, object>("m1",1))
            };
            var cloneDictionary = new Dictionary<IDocument, IEnumerable<KeyValuePair<string, object>>>();
            foreach (var document in documents)
            {
                document
                    .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => cloneDictionary[document] = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            }
            DirectoryMetadata directoryMetadata = new DirectoryMetadata();

            // When
            var returnedDocuments = directoryMetadata.Execute(documents, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(2, returnedDocuments.Count);
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

        private IDocument GetDocumentWithMetadata(string relativePath, params KeyValuePair<string, object>[] metadata)
        {
            IDocument document = Substitute.For<IDocument>();
            IMetadata metadataObject = Substitute.For<IMetadata>();

            metadataObject.GetEnumerator().Returns(metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());

            document.Source.Returns(Path.Combine(ROOT, "input", relativePath));
            document.Metadata.Returns(metadataObject);
            document.GetEnumerator().Returns(metadata.OfType<KeyValuePair<string, object>>().GetEnumerator());

            foreach (var m in metadata)
            {
                metadataObject[m.Key].Returns(m.Value);
                metadataObject.ContainsKey(Arg.Any<string>()).Returns((x => metadata.Any(y => y.Key == (string)x[0])));

                document[m.Key].Returns(m.Value);
                document.ContainsKey(Arg.Any<string>()).Returns((x => metadata.Any(y => y.Key == (string)x[0])));
            }

            return document;
        }
    }
}
