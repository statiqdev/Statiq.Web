using System;
using System.Linq;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Testing;

namespace Wyam.Configuration.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class ConfiguratorTests : BaseFixture
    {
        public class ConfigureMethodTests : ConfiguratorTests
        {
            [Test]
            public void ErrorInDeclarationsContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
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
                    configurator.Configure(configScript, false, false, null);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(2, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 3", exception.InnerExceptions[0].Message);
                StringAssert.StartsWith("Line 3", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInDeclarationsWithEmptyLinesContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
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
                    configurator.Configure(configScript, false, false, null);
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
            public void ErrorInDeclarationsWithoutSetupContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
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
                    configurator.Configure(configScript, false, false, null);
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
                Configurator configurator = GetConfigurator();
                string configScript = @"

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
                    configurator.Configure(configScript, false, false, null);
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
            public void ErrorInConfigWithoutDeclarationsContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                string configScript = @"
int z = 0;

foo bar;
";

                // When
                AggregateException exception = null;
                try
                {
                    configurator.Configure(configScript, false, false, null);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 4", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                string configScript = @"
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
                    configurator.Configure(configScript, false, false, null);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 8", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionOnNewLineContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                string configScript = @"
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
                    configurator.Configure(configScript, false, false, null);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 8", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void ErrorInConfigAfterLambdaExpansionWithArgumentSeparatorNewLinesContainsCorrectLineNumbers()
            {
                // Given
                RemoveListener();
                Configurator configurator = GetConfigurator();
                string configScript = @"
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
                    configurator.Configure(configScript, false, false, null);
                }
                catch (AggregateException ex)
                {
                    exception = ex;
                }

                // Then
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                StringAssert.StartsWith("Line 9", exception.InnerExceptions[0].Message);
            }

            [Test]
            public void CanSetCustomDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
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
DocumentFactory = new CustomDocumentFactory<MyDocument>(DocumentFactory);
";

                // When
                configurator.Configure(configScript, false, false, null);

                // Then
                Assert.AreEqual("CustomDocumentFactory`1", engine.DocumentFactory.GetType().Name);
            }

            [Test]
            public void SetCustomDocumentTypeSetsDocumentFactory()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
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
                configurator.Configure(configScript, false, false, null);

                // Then
                Assert.AreEqual("CustomDocumentFactory`1", engine.DocumentFactory.GetType().Name);
            }

            [Test]
            public void SetsPrimitiveMetadata()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                string configScript = @"
                    InitialMetadata[""TestString""] = ""teststring"";
                    InitialMetadata[""TestInt""] = 1234;
                    InitialMetadata[""TestFloat""] = 1234.567;
                    InitialMetadata[""TestBool""] = true;
                ";

                // When
                configurator.Configure(configScript, false, false, null);

                // Then
                Assert.AreEqual("teststring", engine.InitialMetadata["TestString"]);
                Assert.AreEqual(1234, engine.InitialMetadata["TestInt"]);
                Assert.AreEqual(1234.567, engine.InitialMetadata["TestFloat"]);
                Assert.AreEqual(true, engine.InitialMetadata["TestBool"]);
            }

            [Test]
            public void AddsPipelineAndModules()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                string configScript = @"
                    Pipelines.Add(
                        new ReadFiles(""*.cshtml""),
	                    new WriteFiles("".html""));
                ";

                // When
                configurator.Configure(configScript, false, false, null);

                // Then
                Assert.AreEqual(1, engine.Pipelines.Count);
                Assert.AreEqual(2, engine.Pipelines.Values.First().Count);
            }

            [Test]
            public void SupportsGlobalConstructorMethods()
            {
                // Given
                Engine engine = new Engine();
                Configurator configurator = GetConfigurator(engine);
                string configScript = @"
                    Pipelines.Add(
                        ReadFiles(""*.cshtml""),
	                    WriteFiles("".html""));
                ";

                // When
                configurator.Configure(configScript, false, false, null);

                // Then
                Assert.AreEqual(1, engine.Pipelines.Count);
                Assert.AreEqual(2, engine.Pipelines.Values.First().Count);
            }
        }

        private Configurator GetConfigurator()
        {
            Engine engine = new Engine();
            return GetConfigurator(engine);
        }

        private Configurator GetConfigurator(Engine engine)
        {
            Configurator configurator = new Configurator(engine);
            return configurator;
        }
    }
}
