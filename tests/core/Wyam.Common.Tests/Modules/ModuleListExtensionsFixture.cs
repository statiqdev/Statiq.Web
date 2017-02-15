using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Common.Tests.Modules
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ModuleListExtensionsFixture : BaseFixture
    {
        public class InsertAfterFirstTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterFirst()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertAfterFirst<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertBeforeFirstTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeFirst()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertBeforeFirst<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertAfterLastTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterLast()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertAfterLast<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class InsertBeforeLastTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeLast()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                // When
                collection.InsertBeforeLast<ReadFiles>(new CountModule("foo"));

                // Then
                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }
        }

        public class ReplaceFirstTests : ModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceFirst()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

                // When
                collection.ReplaceFirst<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.AreEqual("replacedKey", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[2]).ValueKey);
            }
        }

        public class ReplaceLastTests : ModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceLast()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

                // When
                collection.ReplaceLast<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("replacedKey", ((CountModule)collection[2]).ValueKey);
            }
        }

        public class InsertMultipleTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertMultiple()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                // When
                collection.InsertBefore("Second", new CountModule("mykey3"), new CountModule("mykey4"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey4", ((CountModule)collection[2]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[3]).ValueKey);
            }
        }

        public class InsertBeforeWithNameTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertBeforeWithName()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                // When
                collection.InsertBefore("Second", new CountModule("mykey3"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[2]).ValueKey);
            }
        }

        public class InsertAfterWithNameTests : ModuleListExtensionsFixture
        {
            [Test]
            public void InsertAfterWithName()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                // When
                collection.InsertAfter("Second", new CountModule("mykey3"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[2]).ValueKey);
            }
        }

        public class ReplaceWithNameTests : ModuleListExtensionsFixture
        {
            [Test]
            public void ReplaceWithName()
            {
                // Given
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                // When
                collection.Replace("Second", new CountModule("mykey3"));

                // Then
                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
            }

            [Test]
            public void AlsoWorksWithModules()
            {
                // Given
                IPipeline collection  = new Pipeline("Test", new []
                {
                    new CountModule("mykey1")
                        .WithName("First"),
                    new CountModule("mykey2")
                        .WithName("Second"),
                    new Concat(new CountModule("mysubkey1").WithName("inner"))
                        .WithName("Third")
                });

                // When
                (collection["Third"] as IModuleList)
                    .Replace("inner", new CountModule("newsubkey"));

                // Then
                Assert.AreEqual("newsubkey", ((CountModule) ((IModuleList) collection["Third"])["inner"]).ValueKey);
            }
        }
    }
}
