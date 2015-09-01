using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
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
            List<string> content = new List<string>();
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
                content.Add(d.Content);
                currentPage.Add(c.Get<int>(MetadataKeys.CurrentPage));
                totalPages.Add(c.Get<int>(MetadataKeys.TotalPages));
                hasNextPage.Add(c.Get<bool>(MetadataKeys.HasNextPage));
                hasPreviousPage.Add(c.Get<bool>(MetadataKeys.HasPreviousPage));
                return null;
            }));
            engine.Pipelines.Add(count, paginate);

            // When
            engine.Execute();

            // Then
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5", "6", "7", "8" }, content);
            CollectionAssert.AreEqual(new[] { 1, 1, 1, 2, 2, 2, 3, 3 }, currentPage);
            CollectionAssert.AreEqual(new[] { 3, 3, 3, 3, 3, 3, 3, 3 }, totalPages);
            CollectionAssert.AreEqual(new[] { true, true, true, true, true, true, false, false }, hasNextPage);
            CollectionAssert.AreEqual(new[] { false, false, false, true, true, true, true, true }, hasPreviousPage);
        }
    }
}
