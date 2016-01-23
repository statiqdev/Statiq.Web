using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration;
using Wyam.Core.IO;
using Wyam.Core.Meta;
using Wyam.Core.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ConfigErrorsFixture
    {
        [SetUp]
        public void SetUp()
        {
            System.Diagnostics.TraceListener testRaceListener =
                Trace.GetListeners().OfType<TestTraceListener>().FirstOrDefault();
            if (testRaceListener != null)
            {
                Trace.RemoveListener(testRaceListener);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Trace.AddListener(new TestTraceListener());
        }

        [Test]
        public void ErrorInSetupContainsCorrectLineNumbers()
        {
            // Given
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
