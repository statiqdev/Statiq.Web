using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [NonParallelizable]
    public class ProcessShortcodesFixture : BaseFixture
    {
        public class ExecuteTests : ProcessShortcodesFixture
        {
            [Test]
            public void ProcessesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Bar");
                IDocument document = new TestDocument("123<?# Bar /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123Foo456");
            }

            [Test]
            public void DisposesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<DisposableShortcode>("Bar");
                IDocument document = new TestDocument("123<?# Bar /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                DisposableShortcode.Disposed.ShouldBeTrue();
            }
        }

        public class TestShortcode : IShortcode
        {
            public IShortcodeResult Execute(string[] args, string content, IDocument document, IExecutionContext context)
            {
                return context.GetShortcodeResult(context.GetContentStream("Foo"));
            }
        }

        public class DisposableShortcode : IShortcode, IDisposable
        {
            public static bool Disposed { get; set; }

            public DisposableShortcode()
            {
                // Make sure it resets
                Disposed = false;
            }

            public IShortcodeResult Execute(string[] args, string content, IDocument document, IExecutionContext context)
            {
                return context.GetShortcodeResult(context.GetContentStream("Foo"));
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }
    }
}
