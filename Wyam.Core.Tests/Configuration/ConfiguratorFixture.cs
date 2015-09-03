using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Core.Configuration;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    public class ConfiguratorFixture
    {
        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
===
=C
D";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiterWithTrailingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
===  
=C
D";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsConfigWithDelimiterWithLeadingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
  ===
=C
D";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"A=
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
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B

===

=C
D";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"=C
D", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsConfigWithoutDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
C";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"A=
=B
C", configParts.Item3);
            
        }





        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiter()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
===
=C
D
---
-E
F";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.AreEqual(@"=C
D", configParts.Item2);
            Assert.AreEqual(@"-E
F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiterWithTrailingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
===  
=C
D
---   
E
=F";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.AreEqual(@"=C
D", configParts.Item2);
            Assert.AreEqual(@"E
=F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnsDeclarationsWithDelimiterWithLeadingSpaces()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
  ===
=C
D
  ---
-E
F";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.IsNull(configParts.Item2);
            Assert.AreEqual(@"A=
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
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B

===

=C
D

---

E-
-F";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual(@"A=
=B", configParts.Item1);
            Assert.AreEqual(@"=C
D", configParts.Item2);
            Assert.AreEqual(@"E-
-F", configParts.Item3);
        }

        [Test]
        public void GetConfigPartsReturnDeclarationsWithoutSetup()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            string configScript = @"A=
=B
C
---
E-
-F";

            // When
            Tuple<string, string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.AreEqual(@"A=
=B
C", configParts.Item2);
            Assert.AreEqual(@"E-
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
        public void GenerateScriptGeneratesCorrectScript(string input, string output)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            HashSet<Type> moduleTypes = new HashSet<Type> {typeof (Content)};
            string expected = $@"

                public static class ConfigScript
                {{
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines, string RootFolder, string InputFolder, string OutputFolder)
                    {{
                        {output}
                    }}
                        public static Wyam.Core.Modules.Content Content(object content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(System.Func<Wyam.Common.IExecutionContext, object> content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(System.Func<Wyam.Common.IDocument, Wyam.Common.IExecutionContext, object> content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(params Wyam.Common.IModule[] modules)
                        {{
                            return new Wyam.Core.Modules.Content(modules);  
                        }}}}";

            // When
            string actual = configurator.GenerateScript(null, input, moduleTypes, new HashSet<string>());

            // Then
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateModuleConstructorMethodsGeneratesOverloadedConstructors()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            Type moduleType = typeof (Content);
            string expected = $@"
                        public static Wyam.Core.Modules.Content Content(object content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(System.Func<Wyam.Common.IExecutionContext, object> content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(System.Func<Wyam.Common.IDocument, Wyam.Common.IExecutionContext, object> content)
                        {{
                            return new Wyam.Core.Modules.Content(content);  
                        }}
                        public static Wyam.Core.Modules.Content Content(params Wyam.Common.IModule[] modules)
                        {{
                            return new Wyam.Core.Modules.Content(modules);  
                        }}";

            // When
            string generated = configurator.GenerateModuleConstructorMethods(moduleType);

            // Then
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void GenerateModuleConstructorMethodsGeneratesGenericConstructors()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Configurator configurator = new Configurator(engine);
            Type moduleType = typeof(GenericModule<>);
            string expected = $@"
                        public static Wyam.Core.Tests.Configuration.GenericModule<T> GenericModule<T>(T input)
                        {{
                            return new Wyam.Core.Tests.Configuration.GenericModule<T>(input);  
                        }}
                        public static Wyam.Core.Tests.Configuration.GenericModule<T> GenericModule<T>(System.Func<Wyam.Common.IDocument, T> input)
                        {{
                            return new Wyam.Core.Tests.Configuration.GenericModule<T>(input);  
                        }}";

            // When
            string generated = configurator.GenerateModuleConstructorMethods(moduleType);

            // Then
            Assert.AreEqual(expected, generated);
        }
    }

    public class GenericModule<T> : IModule
    {
        public GenericModule(T input)
        {

        }

        public GenericModule(Func<IDocument, T> input)
        {

        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
