using System;
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
    public class ConfigClassTests : BaseFixture
    {
        public class ConfigureMethodTests : ConfigClassTests
        {
            [Test]
            public void ErrorInSetupContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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
                FileSystem fileSystem = new FileSystem();
                InitialMetadata initialMetadata = new InitialMetadata();
                PipelineCollection pipelines = new PipelineCollection();
                Config config = new Config(fileSystem, initialMetadata, pipelines);
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

        }
    }
}
