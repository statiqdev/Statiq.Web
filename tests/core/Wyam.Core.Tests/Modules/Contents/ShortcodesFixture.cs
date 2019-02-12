using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [NonParallelizable]
    public class ShortcodesFixture : BaseFixture
    {
        public class ExecuteTests : ShortcodesFixture
        {
            [Test]
            public void ProcessesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Bar");
                IDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123Foo456");
            }

            [Test]
            public void ShortcodeSupportsNullStreamResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<NullStreamShortcode>("Bar");
                IDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123456");
            }

            [Test]
            public void ShortcodeSupportsNullResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("S1");
                context.Shortcodes.Add<NullResultShortcode>("S2");
                IDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789<?# S1 /?>");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123Foo456789Foo");
            }

            [Test]
            public void DisposesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<DisposableShortcode>("Bar");
                IDocument document = new TestDocument("123<?# Bar /?>456");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                DisposableShortcode.Disposed.ShouldBeTrue();
            }

            [Test]
            public void ShortcodesCanAddMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<AddsMetadataShortcode>("S1");
                context.Shortcodes.Add<AddsMetadataShortcode2>("S2");
                IDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123456789");
                results.Single()["A"].ShouldBe("3");
                results.Single()["B"].ShouldBe("2");
                results.Single()["C"].ShouldBe("4");
            }

            [Test]
            public void ShortcodesCanReadMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<ReadsMetadataShortcode>("S1");
                context.Shortcodes.Add<ReadsMetadataShortcode>("S2");
                IDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789<?# S1 /?>", new MetadataItems
                {
                    { "Foo", 10 }
                });
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123456789");
                results.Single()["Foo"].ShouldBe(13);
            }

            [Test]
            public void ShortcodesPersistState()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<IncrementingShortcode>("S");
                IDocument document = new TestDocument("123<?# S /?>456<?# S /?>789<?# S /?>");
                Core.Modules.Contents.Shortcodes module = new Core.Modules.Contents.Shortcodes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe("123456789");
                results.Single()["Foo"].ShouldBe(22);
            }
        }

        public class TestShortcode : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult(context.GetContentStream("Foo"));
        }

        public class NullStreamShortcode : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult((Stream)null);
        }

        public class NullResultShortcode : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) => null;
        }

        public class DisposableShortcode : IShortcode, IDisposable
        {
            public static bool Disposed { get; set; }

            public DisposableShortcode()
            {
                // Make sure it resets
                Disposed = false;
            }

            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult(context.GetContentStream("Foo"));

            public void Dispose() =>
                Disposed = true;
        }

        public class AddsMetadataShortcode : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult((Stream)null, new MetadataItems
                {
                    { "A", "1" },
                    { "B", "2" }
                });
        }

        public class AddsMetadataShortcode2 : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult((Stream)null, new MetadataItems
                {
                    { "A", "3" },
                    { "C", "4" }
                });
        }

        public class ReadsMetadataShortcode : IShortcode
        {
            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult((Stream)null, new MetadataItems
                {
                    { $"Foo", document.Get<int>("Foo") + 1 }
                });
        }

        public class IncrementingShortcode : IShortcode
        {
            private int _value = 20;

            public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                context.GetShortcodeResult((Stream)null, new MetadataItems
                {
                    { $"Foo", _value++ }
                });
        }
    }
}
