using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Configuration.ConfigScript;
using Wyam.Core.Modules.Contents;
using Wyam.Testing;

namespace Wyam.Configuration.Tests.ConfigScript
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ScriptManagerTests : BaseFixture
    {
        public class PreprocessMethodTests : ScriptManagerTests
        {
            [TestCase(@"Pipelines.Add(Content())", @"Pipelines.Add(Content())")]
            [TestCase(@"Pipelines.Add(Content(Content()))", @"Pipelines.Add(Content(Content()))")]
            [TestCase(@"Pipelines.Add(Content(@doc => @doc.Foo()))", @"Pipelines.Add(Content(@doc => @doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(), Foobar())", @"Pipelines.Add(Content(), Foobar())")]
            [TestCase(@"Pipelines.Add(Content(""foobar""))", @"Pipelines.Add(Content(""foobar""))")]
            [TestCase(@"Pipelines.Add(Content((x) => x.Foo()))", @"Pipelines.Add(Content((x) => x.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo()))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc2.Foo()))", @"Pipelines.Add(Content((@doc2,_)=>@doc2.Foo()))")]
            [TestCase(@"Pipelines.Add(Content((int)@doc.Foo()))", @"Pipelines.Add(Content((@doc,_)=>(int)@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@ctx.Foo()))", @"Pipelines.Add(Content(@ctx=>@ctx.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content())))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo(Content())))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content(@doc.Foo()))))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo(Content(@doc.Foo()))))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo(Content(@doc2.Foo()))))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo(Content((@doc2,_)=>@doc2.Foo()))))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo(@ctx.Bar)))", @"Pipelines.Add(Content((@doc,@ctx)=>@doc.Foo(@ctx.Bar)))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo(@ctx2.Bar)))", @"Pipelines.Add(Content((@doc,@ctx2)=>@doc.Foo(@ctx2.Bar)))")]
            [TestCase(@"Pipelines.Add(Content(@doc[""foo""]))", @"Pipelines.Add(Content((@doc,_)=>@doc[""foo""]))")]
            [TestCase(@"Pipelines.Add(Content(@ctx[""foo""]))", @"Pipelines.Add(Content(@ctx=>@ctx[""foo""]))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo))")]
            [TestCase(@"Pipelines.Add(Content().Where(@doc.Foo()))", @"Pipelines.Add(Content().Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content().Where().Where(@doc.Foo()))", @"Pipelines.Add(Content().Where().Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content().Where(5).Where(@doc.Foo()))", @"Pipelines.Add(Content().Where(5).Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content().Where(@doc[""foo""]))", @"Pipelines.Add(Content().Where((@doc,_)=>@doc[""foo""]))")]
            [TestCase(@"Pipelines.Add(Content().Where(@doc.Foo))", @"Pipelines.Add(Content().Where((@doc,_)=>@doc.Foo))")]
            [TestCase(@"Pipelines.Add(Content(""foobar"").Where(@doc.Foo()))", @"Pipelines.Add(Content(""foobar"").Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc.Foo()).Where(@doc.Foo()))", @"Pipelines.Add(Content((@doc,_)=>@doc.Foo()).Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc[""foo""]).Where(@doc.Foo()))", @"Pipelines.Add(Content((@doc,_)=>@doc[""foo""]).Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(@doc.@Foo).Where(@doc.Foo()))", @"Pipelines.Add(Content((@doc,_)=>@doc.@Foo).Where((@doc,_)=>@doc.Foo()))")]
            [TestCase(@"Pipelines.Add(Content(Bar(@doc.Foo())))", @"Pipelines.Add(Content((@doc,_)=>Bar(@doc.Foo())))")]
            [TestCase(@"Pipelines.Add(Content(Bar(@ctx.Foo())))", @"Pipelines.Add(Content(@ctx=>Bar(@ctx.Foo())))")]
            [TestCase(@"Pipelines.Add(Content(Bar(@ctx2.Foo())))", @"Pipelines.Add(Content(@ctx2=>Bar(@ctx2.Foo())))")]
            [TestCase(@"Pipelines.Add(Content().Where(Bar(@doc.Foo())))", @"Pipelines.Add(Content().Where((@doc,_)=>Bar(@doc.Foo())))")]
            [TestCase(@"Pipelines.Add(Content().Where(Bar(@ctx.Foo())))", @"Pipelines.Add(Content().Where(@ctx=>Bar(@ctx.Foo())))")]
            [TestCase(@"Pipelines.Add(Content(Content(""*.md"").Where(@doc.Foo())))", @"Pipelines.Add(Content(Content(""*.md"").Where((@doc,_)=>@doc.Foo())))")]
            public void GeneratesCorrectScript(string input, string output)
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string expected = $@"// Generated: bring all module namespaces in scope
using Foo.Bar;

#line 1
{output}

// Generated: methods for module instantiation
Wyam.Core.Modules.Contents.Content Content(object content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.ContextConfig content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.DocumentConfig content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(params Wyam.Common.Modules.IModule[] modules)
{{
    return new Wyam.Core.Modules.Contents.Content(modules);  
}}";

                // When
                string actual = scriptManager.Preprocess(input, moduleTypes, namespaces, Array.Empty<Assembly>());

                // Then
                Assert.AreEqual(expected, actual);
            }
        }

        public class GenerateModuleConstructorMethodsMethodTests : ScriptManagerTests
        {
            [Test]
            public void GeneratesOverloadedConstructors()
            {
                // Given
                Dictionary<string, string> memberNames = new Dictionary<string, string>();
                string expected = $@"
Wyam.Core.Modules.Contents.Content Content(object content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.ContextConfig content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(Wyam.Common.Configuration.DocumentConfig content)
{{
    return new Wyam.Core.Modules.Contents.Content(content);  
}}
Wyam.Core.Modules.Contents.Content Content(params Wyam.Common.Modules.IModule[] modules)
{{
    return new Wyam.Core.Modules.Contents.Content(modules);  
}}";

                // When
                string generated = ScriptManager.GenerateModuleConstructorMethods(typeof(Content), memberNames);

                // Then
                Assert.AreEqual(expected, generated);
            }

            [Test]
            public void GeneratesGenericConstructors()
            {
                // Given
                Dictionary<string, string> memberNames = new Dictionary<string, string>();
                string expected = $@"
Wyam.Configuration.Tests.ConfigScript.ScriptManagerTests.GenericModule<T> GenericModule<T>(T input)
{{
    return new Wyam.Configuration.Tests.ConfigScript.ScriptManagerTests.GenericModule<T>(input);  
}}
Wyam.Configuration.Tests.ConfigScript.ScriptManagerTests.GenericModule<T> GenericModule<T>(System.Action<T> input)
{{
    return new Wyam.Configuration.Tests.ConfigScript.ScriptManagerTests.GenericModule<T>(input);  
}}";

                // When
                string generated = ScriptManager.GenerateModuleConstructorMethods(typeof(GenericModule<>), memberNames);

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
