using System;
using System.Collections.Generic;
using NUnit.Framework;
using Wyam.Core.Configuration;
using Wyam.Core.IO;
using Wyam.Core.Meta;
using Wyam.Core.Pipelines;
using Wyam.Testing;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ConfigTests : BaseFixture
    {
        public class ConfigureMethodTests : ConfigTests
        {
            [Test]
            public void ErrorInSetupContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
foo
===
class Y { };
---
int z = 0;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(2, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 3", exception.InnerExceptions[0].Message);
                StringAssert.StartsWith("Line 3", exception.InnerExceptions[1].Message);
            }

            [Test]
            public void ErrorInDeclarationsContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
===
class Y { };
foo bar;
---
int z = 0;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(2, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 5", exception.InnerExceptions[0].Message);
                StringAssert.StartsWith("Line 5", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInDeclarationsWithEmptyLinesContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");

===

class Y { };

foo bar;

---

int z = 0;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(2, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 8", exception.InnerExceptions[0].Message);
                StringAssert.StartsWith("Line 8", exception.InnerExceptions[1].Message);
            }

            [Test]
            public void ErrorInDeclarationsWithoutSetupContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
class Y { };

foo bar;

---

int z = 0;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(2, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 4", exception.InnerExceptions[0].Message);
                StringAssert.StartsWith("Line 4", exception.InnerExceptions[1].Message);
            }

            [Test]
            public void ErrorInConfigContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");

===

class Y { };
class X { };

---

int z = 0;

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 13", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigWithoutDeclarationsContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
===
int z = 0;

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 6", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
===
Pipelines.Add(
    Content(true
        && @doc.Get<bool>(""Key"") == false
    )
);

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 10", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionOnNewLineContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
===
Pipelines.Add(
    Content(
        @doc.Get<bool>(""Key"") == false
    )
);

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 10", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionWithArgumentSeparatorNewLinesContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
Assemblies.Load("""");
===
Pipelines.Add(
    If(
        @doc.Get<bool>(""Key""),
        Content(""Baz"")
    )
);

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    config.Configure(configScript, false, null, false);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 11", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void CanSetCustomDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
public class MyDocument : CustomDocument
{
    public int Count { get; set; }

    protected override CustomDocument Clone()
    {
        return new MyDocument();
    }
}
---
Engine.DocumentFactory = new CustomDocumentFactory<MyDocument>(Engine.DocumentFactory);
";

                // When
                config.Configure(configScript, false, null, false);

                // Then
                Assert.AreEqual("CustomDocumentFactory`1", engine.DocumentFactory.GetType().Name);
            }

            [Test]
            public void SetCustomDocumentTypeSetsDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                FileSystem fileSystem = new FileSystem();
                Config config = new Config(engine, fileSystem);
                string configScript = @"
public class MyDocument : CustomDocument
{
    public int Count { get; set; }

    protected override CustomDocument Clone()
    {
        return new MyDocument();
    }
}
---
SetCustomDocumentType<MyDocument>();
";

                // When
                config.Configure(configScript, false, null, false);

                // Then
                Assert.AreEqual("CustomDocumentFactory`1", engine.DocumentFactory.GetType().Name);
            }
        }
    }
}
