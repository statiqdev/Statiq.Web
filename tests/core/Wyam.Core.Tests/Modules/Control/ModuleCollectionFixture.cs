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

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ModuleCollectionFixture : BaseFixture
    {
        public class ExecuteTests : ModuleCollectionFixture
        {
            [Test]
            public void ChildModulesAreExecuted()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3
                };
                engine.Pipelines.Add(a, new Core.Modules.Control.ModuleCollection(b, c));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(6, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(24, c.OutputCount);
            }
        }

        public class ModuleCollectionExtensionTests : BaseFixture
        {
            [Test]
            public void InsertAfterFirst()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                collection.InsertAfterFirst<ReadFiles>(new CountModule("foo"));

                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }

            [Test]
            public void InsertBeforeFirst()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                collection.InsertBeforeFirst<ReadFiles>(new CountModule("foo"));

                Assert.AreEqual(collection[0].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }


            [Test]
            public void InsertAfterLast()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                collection.InsertAfterLast<ReadFiles>(new CountModule("foo"));

                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[2].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }

            [Test]
            public void InsertBeforeLast()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new ReadFiles(ctx => "*.md"),
                    new WriteFiles()
                });

                collection.InsertBeforeLast<ReadFiles>(new CountModule("foo"));

                Assert.AreEqual(collection[0].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[1].GetType(), typeof(CountModule));
                Assert.AreEqual(collection[2].GetType(), typeof(ReadFiles));
                Assert.AreEqual(collection[3].GetType(), typeof(WriteFiles));
            }

            [Test]
            public void ReplaceFirst()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

                collection.ReplaceFirst<CountModule>(new CountModule("replacedKey"));

                Assert.AreEqual("replacedKey", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[2]).ValueKey);
            }

            [Test]
            public void ReplaceLast()
            {
                IPipeline collection = new Pipeline("Test", new IModule[]
                {
                    new ReadFiles(ctx => "*.md"),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new WriteFiles()
                });

                collection.ReplaceLast<CountModule>(new CountModule("replacedKey"));

                Assert.AreEqual("mykey1", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("replacedKey", ((CountModule)collection[2]).ValueKey);
            }

            [Test]
            public void InsertMultiple()
            {
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                collection.InsertBefore("Second", new CountModule("mykey3"), new CountModule("mykey4"));

                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey4", ((CountModule)collection[2]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[3]).ValueKey);
            }

            [Test]
            public void InsertBeforeWithName()
            {
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                collection.InsertBefore("Second", new CountModule("mykey3"));

                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[2]).ValueKey);
            }

            [Test]
            public void InsertAfterWithName()
            {
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                collection.InsertAfter("Second", new CountModule("mykey3"));

                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey2", ((CountModule)collection[1]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[2]).ValueKey);
            }


            [Test]
            public void ReplaceWithName()
            {
                IPipeline collection = new Pipeline("Test", new[]
                {
                    new CountModule("mykey1").WithName("First"),
                    new CountModule("mykey2").WithName("Second")
                });

                collection.Replace("Second", new CountModule("mykey3"));

                Assert.AreEqual("mykey1", ((CountModule)collection[0]).ValueKey);
                Assert.AreEqual("mykey3", ((CountModule)collection[1]).ValueKey);
            }

            [Test]
            public void AlsoWorksWithModules()
            {
                IPipeline collection  = new Pipeline("Test", new []
                {
                    new CountModule("mykey1")
                        .WithName("First"),
                    new CountModule("mykey2")
                        .WithName("Second"),
                    new Concat(new CountModule("mysubkey1").WithName("inner"))
                        .WithName("Third")
                });

                (collection["Third"] as IModuleList)
                    .Replace("inner", new CountModule("newsubkey"));

                Assert.AreEqual("newsubkey", ((CountModule) ((IModuleList) collection["Third"])["inner"]).ValueKey);
            }
        }
    }
}
