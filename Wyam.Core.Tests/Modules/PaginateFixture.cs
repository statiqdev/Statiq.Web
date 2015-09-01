using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Core.Documents;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class PaginateFixture
    {
        [Test]
        public void PaginateSetsCorrectMetadata()
        {
            // Given
            List<int> currentPage = new List<int>();
            List<int> totalPages = new List<int>();
            List<bool> hasNextPage = new List<bool>();
            List<bool> hasPreviousPage = new List<bool>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 7
            };
            Paginate paginate = new Paginate(3, new Execute((d, c) =>
            {
                currentPage.Add(d.Get<int>(MetadataKeys.CurrentPage));
                totalPages.Add(d.Get<int>(MetadataKeys.TotalPages));
                hasNextPage.Add(d.Get<bool>(MetadataKeys.HasNextPage));
                hasPreviousPage.Add(d.Get<bool>(MetadataKeys.HasPreviousPage));
                return null;
            }));
            engine.Pipelines.Add(count, paginate);

            // When
            engine.Execute();

            // Then
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, currentPage);
            CollectionAssert.AreEqual(new[] { 3, 3, 3 }, totalPages);
            CollectionAssert.AreEqual(new[] { true, true, false }, hasNextPage);
            CollectionAssert.AreEqual(new[] { false, true, true }, hasPreviousPage);
        }

        [Test]
        public void PaginateSetsDocumentsInMetadata()
        {
            // Given
            List<IList<string>> content = new List<IList<string>>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 7
            };
            Paginate paginate = new Paginate(3, new Execute((d, c) =>
            {
                content.Add(d.Get<IList<IDocument>>(MetadataKeys.PageDocuments).Select(x => x.Content).ToList());
                return null;
            }));
            engine.Pipelines.Add(count, paginate);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(3, content.Count);
            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, content[0]);
            CollectionAssert.AreEqual(new[] { "4", "5", "6" }, content[1]);
            CollectionAssert.AreEqual(new[] { "7", "8" }, content[2]);
        }
    }
}
