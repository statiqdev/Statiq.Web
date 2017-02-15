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
using Wyam.Common.Modules;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class TreeFixture : BaseFixture
    {
        public class ExecuteTests : TreeFixture
        {
            [Test]
            public void GetsTreeWithCommonRoot()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context, 
                    "root/a/2.txt",
                    "root/b/3.txt",
                    "root/a/1.txt",
                    "root/b/x/4.txt",
                    "root/c/d/5.txt",
                    "root/6.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                Assert.AreEqual(1, documents.Count);
                AssertTree(documents[0],
                    "index.html",
                    "root/index.html",
                    "root/6.txt",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/b/3.txt",
                    "root/b/x/index.html",
                    "root/b/x/4.txt",
                    "root/c/index.html",
                    "root/c/d/index.html",
                    "root/c/d/5.txt");
            }

            [Test]
            public void GetsTree()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                Assert.AreEqual(1, documents.Count);
                AssertTree(documents[0],
                    "index.html",
                    "6.txt",
                    "a/index.html",
                    "a/1.txt",
                    "a/2.txt",
                    "b/index.html",
                    "b/3.txt",
                    "b/x/index.html",
                    "b/x/4.txt",
                    "c/index.html",
                    "c/d/index.html",
                    "c/d/5.txt");
            }

            [Test]
            public void CollapseRoot()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt"
                );
                Tree tree = new Tree().WithNesting(true, true);

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                Assert.AreEqual(4, documents.Count);
                CollectionAssert.AreEquivalent(
                    new[] {"a/index.html", "b/index.html", "c/index.html", "6.txt"},
                    documents.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath));
            }

            [Test]
            public void GetsPreviousSibling()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                IDocument document = FindTreeNode(documents[0], "root/a/2.txt");
                Assert.AreEqual("root/a/1.txt",
                    document.Document(Keys.PreviousSibling).FilePath(Keys.RelativeFilePath).FullPath);
            }

            [Test]
            public void GetsNextSibling()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                IDocument document = FindTreeNode(documents[0], "root/a/2.txt");
                Assert.AreEqual("root/a/3.txt",
                    document.Document(Keys.NextSibling).FilePath(Keys.RelativeFilePath).FullPath);
            }

            [Test]
            public void GetsPrevious()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                IDocument document = FindTreeNode(documents[0], "root/a/2.txt");
                Assert.AreEqual("root/a/1.txt",
                    document.Document(Keys.Previous).FilePath(Keys.RelativeFilePath).FullPath);
            }

            [Test]
            public void GetsPreviousUpTree()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt",
                    "root/b/4.txt"
                );
                Tree tree = new Tree().WithNesting();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                IDocument document = FindTreeNode(documents[0], "root/b/4.txt");
                Assert.AreEqual("root/b/index.html",
                    document.Document(Keys.Previous).FilePath(Keys.RelativeFilePath).FullPath);
            }


            [Test]
            public void SplitsTree()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/a/1.txt",
                    "root/b/4.txt"
                );
                Tree tree = new Tree()
                    .WithNesting()
                    .WithRoots((doc, ctx) => doc.FilePath(Keys.RelativeFilePath).FullPath.EndsWith("b/index.html"));

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                Assert.AreEqual(2, documents.Count);
                AssertTree(documents[0],
                    "root/b/index.html",
                    "root/b/4.txt");
                AssertTree(documents[1],
                    "index.html",
                    "root/index.html",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt");
            }

            [Test]
            public void FlatTree()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                IDocument[] inputs = GetDocuments(context,
                    "root/a/b/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt"
                );
                Tree tree = new Tree();

                // When
                List<IDocument> documents = tree.Execute(inputs, context).ToList();

                // Then
                AssertTreeChildren(documents[0],
                    "root/a/b/2.txt");
                AssertTreeChildren(documents[1],
                    "root/a/3.txt");
                AssertTreeChildren(documents[2],
                    "root/a/1.txt");
                AssertTreeChildren(documents[3],
                    "root/a/b/index.html",
                    "root/a/b/2.txt");
                AssertTreeChildren(documents[4],
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/3.txt",
                    "root/a/b/index.html");
                AssertTreeChildren(documents[5],
                    "root/index.html",
                    "root/a/index.html");
            }

            private IDocument FindTreeNode(IDocument first, string relativeFilePath)
            {
                while (first != null && first.FilePath(Keys.RelativeFilePath).FullPath != relativeFilePath)
                {
                    first = first.Document(Keys.Next);
                }
                return first;
            }

            private void AssertTree(IDocument first, params string[] relativeFilePaths)
            {
                foreach (string relativeFilePath in relativeFilePaths)
                {
                    Assert.IsNotNull(first);
                    Assert.AreEqual(relativeFilePath, first.FilePath(Keys.RelativeFilePath).FullPath);
                    first = first.Document(Keys.Next);
                }
            }

            private void AssertTreeChildren(IDocument parent, string parentPath, params string[] childFilePaths)
            {
                Assert.IsNotNull(parent);
                Assert.AreEqual(parentPath, parent.FilePath(Keys.RelativeFilePath).FullPath);
                IReadOnlyList<IDocument> children = parent.DocumentList(Keys.Children);
                CollectionAssert.AreEqual(childFilePaths, children.Select(x => x.FilePath(Keys.RelativeFilePath).FullPath).ToArray());
            }

            private IDocument[] GetDocuments(IExecutionContext context, params string[] relativeFilePaths) =>
                relativeFilePaths.Select(x => context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.RelativeFilePath, x)
                })).ToArray();
        }
    }
}
