using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Configuration;
using Wyam.Core.Configuration;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ConfigCompilationTests : BaseFixture
    {
        public class GenerateMethodTests : ConfigCompilationTests
        { 
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
            public void GeneratesCorrectScript(string input, string output)
            {
                // Given
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = Array.Empty<string>();
                string expected = $@"

                public class ConfigScript : ConfigScriptBase
                {{
                    public ConfigScript(Engine engine) : base(engine) {{ }}

                    public override void Run()
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
                string actual = ConfigCompilation.Generate(null, input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }
        }

        public class GenerateModuleConstructorMethodsMethodTests : ConfigCompilationTests
        {
            [Test]
            public void GeneratesOverloadedConstructors()
            {
                // Given
                Dictionary<string, string> memberNames = new Dictionary<string, string>();
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
                string generated = ConfigCompilation.GenerateModuleConstructorMethods(typeof(Content), memberNames);

                // Then
                Assert.AreEqual(expected, generated);
            }

            [Test]
            public void GeneratesGenericConstructors()
            {
                // Given
                Dictionary<string, string> memberNames = new Dictionary<string, string>();
                string expected = $@"
                        public static Wyam.Core.Tests.Configuration.ConfigScriptTests.GenericModule<T> GenericModule<T>(T input)
                        {{
                            return new Wyam.Core.Tests.Configuration.ConfigScriptTests.GenericModule<T>(input);  
                        }}
                        public static Wyam.Core.Tests.Configuration.ConfigScriptTests.GenericModule<T> GenericModule<T>(System.Action<T> input)
                        {{
                            return new Wyam.Core.Tests.Configuration.ConfigScriptTests.GenericModule<T>(input);  
                        }}";

                // When
                string generated = ConfigCompilation.GenerateModuleConstructorMethods(typeof(GenericModule<>), memberNames);

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
}
