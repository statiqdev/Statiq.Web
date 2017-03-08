using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Ploeh.AutoFixture;

using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    public class SerialCacheFixture : BaseFixture
    {
        public class ExecuteTests : SerialCacheFixture
        {
            private static readonly Fixture Autofixture = new Fixture();

            [Test]
            public void During_first_run_passthrough_all_outputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CacheTesterModule appender = Autofixture.Create<CacheTesterModule>();
                IDocument input = new TestDocument(Autofixture.Create<string>());

                SerialCache combine = new SerialCache(appender);

                // When
                IDocument result = combine.Execute(new[] {input}, context).Single();

                // Then
                CollectionAssert.AreEqual(input.Content + appender.Text, result.Content);
            }

            [Test]
            public void During_cached_pass_use_exiting_outputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CacheTesterModule appender = Autofixture.Create<CacheTesterModule>();
                IDocument input = new TestDocument(Autofixture.Create<string>());

                SerialCache combine = new SerialCache(appender);
                combine.Execute(new[] { input }, context).Enumerate();

                // When
                IDocument result = combine.Execute(new[] { input }, context).Single();

                // Then
                CollectionAssert.AreEqual(input.Content + appender.Text, result.Content);
            }

            [Test]
            public void During_cached_pass_do_not_re_execute()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CacheTesterModule appender = Autofixture.Create<CacheTesterModule>();
                IDocument input = new TestDocument(Autofixture.Create<string>());

                SerialCache combine = new SerialCache(appender);
                combine.Execute(new[] { input }, context).Enumerate();

                // When
                combine.Execute(new[] { input }, context).Enumerate();

                // Then
                Assert.AreEqual(1, appender.ExecuteCount);
            }
        }

        private class CacheTesterModule : IModule
        {
            public string Text { get; }

            public int ExecuteCount { get; private set; }

            public CacheTesterModule(string text)
            {
                Text = text;
            }

            public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
            {
                ExecuteCount++;
                foreach (IDocument document in inputs)
                {
                    yield return context.GetDocument(document, document.Content + Text);
                }
            }
        }
    }
}