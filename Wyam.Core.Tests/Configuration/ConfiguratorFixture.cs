using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Configuration;
using Wyam.Core.Modules;
using Wyam.Core.Modules.Contents;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ConfiguratorFixture
    {
        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
===
=C
D";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 4
=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiterWithTrailingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
===  
=C
D";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 4
=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsConfigWithDelimiterWithLeadingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
  ===
=C
D";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 1
A=
=B
  ===
=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiterWithExtraLines()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B

===

=C
D";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B
", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 5

=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsConfigWithoutDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
C";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 1
A=
=B
C", configParts.Item3);
            
        }





        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
===
=C
D
---
-E
F";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B", configParts.Item1);
            Assert.AreEqual(@"#line 4
=C
D", configParts.Item2);
            Assert.AreEqual(@"#line 7
-E
F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiterWithTrailingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
===  
=C
D
---   
E
=F";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B", configParts.Item1);
            Assert.AreEqual(@"#line 4
=C
D", configParts.Item2);
            Assert.AreEqual(@"#line 7
E
=F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiterWithLeadingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
  ===
=C
D
  ---
-E
F";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"#line 1
A=
=B
  ===
=C
D
  ---
-E
F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiterWithExtraLines()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B

===

=C
D

---

E-
-F";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"#line 1
A=
=B
", configParts.Item1);
            Assert.AreEqual(@"#line 5

=C
D
", configParts.Item2);
            Assert.AreEqual(@"#line 10

E-
-F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnDeclarationsWithoutSetup()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            string configScript = @"A=
=B
C
---
E-
-F";

            // When
            Tuple<string, string, string> configParts = config.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.AreEqual(@"#line 1
A=
=B
C", configParts.Item2);
            Assert.AreEqual(@"#line 5
E-
-F", configParts.Item3);

        }
        
        [TestCase(@"Pipelines.Add(Content())", @"Pipelines.Add(ConfigScript.Content())")]
        [TestCase(@"Pipelines.Add(Content(Content()))", @"Pipelines.Add(ConfigScript.Content(ConfigScript.Content()))")]
        [TestCase(@"Pipelines.Add(Content(@doc => @doc.Foo()))", @"Pipelines.Add(ConfigScript.Content(@doc => @doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(), Foobar())", @"Pipelines.Add(ConfigScript.Content(), Foobar())")]
        [TestCase(@"Pipelines.Add(Content(""foobar""))", @"Pipelines.Add(ConfigScript.Content(""foobar""))")]
        [TestCase(@"Pipelines.Add(Content((x) => x.Foo()))", @"Pipelines.Add(ConfigScript.Content((x) => x.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc2.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc2,_)=>@doc2.Foo()))")]
        [TestCase(@"Pipelines.Add(Content((int)@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>(int)@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@ctx.Foo()))", @"Pipelines.Add(ConfigScript.Content(@ctx=>@ctx.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content())))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo(ConfigScript.Content())))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content(@doc.Foo()))))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo(ConfigScript.Content(@doc.Foo()))))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content(@doc2.Foo()))))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo(ConfigScript.Content((@doc2,_)=>@doc2.Foo()))))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo(@ctx.Bar)))", @"Pipelines.Add(ConfigScript.Content((@doc,@ctx)=>@doc.Foo(@ctx.Bar)))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo(@ctx2.Bar)))", @"Pipelines.Add(ConfigScript.Content((@doc,@ctx2)=>@doc.Foo(@ctx2.Bar)))")]
        [TestCase(@"Pipelines.Add(Content(@doc[""foo""]))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc[""foo""]))")]
        [TestCase(@"Pipelines.Add(Content(@ctx[""foo""]))", @"Pipelines.Add(ConfigScript.Content(@ctx=>@ctx[""foo""]))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo))")]
        [TestCase(@"Pipelines.Add(Content().Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content().Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content().Where().Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content().Where().Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content().Where(5).Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content().Where(5).Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content().Where(@doc[""foo""]))", @"Pipelines.Add(ConfigScript.Content().Where((@doc,_)=>@doc[""foo""]))")]
        [TestCase(@"Pipelines.Add(Content().Where(@doc.Foo))", @"Pipelines.Add(ConfigScript.Content().Where((@doc,_)=>@doc.Foo))")]
        [TestCase(@"Pipelines.Add(Content(""foobar"").Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content(""foobar"").Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc.Foo()).Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.Foo()).Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc[""foo""]).Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc[""foo""]).Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(@doc.@Foo).Where(@doc.Foo()))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>@doc.@Foo).Where((@doc,_)=>@doc.Foo()))")]
        [TestCase(@"Pipelines.Add(Content(Bar(@doc.Foo())))", @"Pipelines.Add(ConfigScript.Content((@doc,_)=>Bar(@doc.Foo())))")]
        [TestCase(@"Pipelines.Add(Content(Bar(@ctx.Foo())))", @"Pipelines.Add(ConfigScript.Content(@ctx=>Bar(@ctx.Foo())))")]
        [TestCase(@"Pipelines.Add(Content(Bar(@ctx2.Foo())))", @"Pipelines.Add(ConfigScript.Content(@ctx2=>Bar(@ctx2.Foo())))")]
        [TestCase(@"Pipelines.Add(Content().Where(Bar(@doc.Foo())))", @"Pipelines.Add(ConfigScript.Content().Where((@doc,_)=>Bar(@doc.Foo())))")]
        [TestCase(@"Pipelines.Add(Content().Where(Bar(@ctx.Foo())))", @"Pipelines.Add(ConfigScript.Content().Where(@ctx=>Bar(@ctx.Foo())))")]
        [TestCase(@"Pipelines.Add(Content(Content(""*.md"").Where(@doc.Foo())))", @"Pipelines.Add(ConfigScript.Content(ConfigScript.Content(""*.md"").Where((@doc,_)=>@doc.Foo())))")]
        public void GenerateScriptGeneratesCorrectScript(string input, string output)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            HashSet<Type> moduleTypes = new HashSet<Type> {typeof (Content)};
            string expected = $@"

                public static class ConfigScript
                {{
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines, string RootFolder, string InputFolder, string OutputFolder)
                    {{
{output}
                    }}
                        public static Wyam.Core.Modules.Contents.Content Content(object content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.ContextConfig content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.DocumentConfig content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(params Wyam.Common.Modules.IModule[] modules)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(modules);  
                        }}}}";

            // When
            string actual = config.GenerateScript(null, input, moduleTypes);

            // Then
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ErrorInSetupContainsCorrectLineNumbers()
        {
            // Given
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
            Engine engine = new Engine();
            Config config = new Config(engine);
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
        public void GenerateModuleConstructorMethodsGeneratesOverloadedConstructors()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            Type moduleType = typeof (Content);
            string expected = $@"
                        public static Wyam.Core.Modules.Contents.Content Content(object content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.ContextConfig content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.DocumentConfig content)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Contents.Content Content(params Wyam.Common.Modules.IModule[] modules)
                        {{
                            return new Wyam.Core.Modules.Contents.Content(modules);  
                        }}";

            // When
            string generated = config.GenerateModuleConstructorMethods(moduleType);

            // Then
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void GenerateModuleConstructorMethodsGeneratesGenericConstructors()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Config config = new Config(engine);
            Type moduleType = typeof(GenericModule<>);
            string expected = $@"
                        public static Wyam.Core.Tests.Configuration.GenericModule<T> GenericModule<T>(T input)
                        {{
                            return new Wyam.Core.Tests.Configuration.GenericModule<T>(input);  
                        }}
                        public static Wyam.Core.Tests.Configuration.GenericModule<T> GenericModule<T>(System.Action<T> input)
                        {{
                            return new Wyam.Core.Tests.Configuration.GenericModule<T>(input);  
                        }}";

            // When
            string generated = config.GenerateModuleConstructorMethods(moduleType);

            // Then
            Assert.AreEqual(expected, generated);
        }
    }

    public class GenericModule<T> : IModule
    {
        public GenericModule(T input)
        {

        }

        public GenericModule(Action<T> input)
        {

        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
