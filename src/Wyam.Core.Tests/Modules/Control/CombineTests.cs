using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules.Control;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class CombineTests : BaseFixture
    {
        public class ExecuteMethodTests : CombineTests
        {
            [Test]
            public void AppendsContent()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .GetDocument(Arg.Any<IDocument>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x =>
                    {
                        IDocument result = Substitute.For<IDocument>();
                        result.Content.Returns(x.ArgAt<string>(1));
                        return result;
                    });
                IDocument a = Substitute.For<IDocument>();
                a.Content.Returns(@"a");
                IDocument b = Substitute.For<IDocument>();
                b.Content.Returns(@"b");
                Combine combine = new Combine();

                // When
                List<IDocument> results = combine.Execute(new[] { a, b }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(new [] { "ab" }, results.Select(x => x.Content));
            }

            [Test]
            public void MergesMetadata()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .GetDocument(Arg.Any<IDocument>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x =>
                    {
                        Dictionary<string, object> metadata = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> kvp in x.ArgAt<IDocument>(0))
                        {
                            metadata[kvp.Key] = kvp.Value;
                        }
                        foreach (KeyValuePair<string, object> kvp in x.ArgAt<IEnumerable<KeyValuePair<string, object>>>(2))
                        {
                            metadata[kvp.Key] = kvp.Value;
                        }
                        IDocument result = Substitute.For<IDocument>();
                        result.GetEnumerator().Returns(metadata.GetEnumerator());
                        return result;
                    });
                IDocument a = Substitute.For<IDocument>();
                a.GetEnumerator().Returns(new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 2 }
                }.GetEnumerator());
                IDocument b = Substitute.For<IDocument>();
                b.GetEnumerator().Returns(new Dictionary<string, object>
                {
                    { "b", 3 },
                    { "c", 4 }
                }.GetEnumerator());
                Combine combine = new Combine();

                // When
                List<IDocument> results = combine.Execute(new[] { a, b }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 3 },
                    { "c", 4 }
                }, Iterate(results.First().GetEnumerator()));
            }
        }

        private IEnumerable Iterate(IEnumerator iterator)
        {
            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }
        }
    }
}
