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
        public class ParseMethodTests : ScriptManagerTests
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
            public void CorrectlyParsesScriptCode(string input, string output)
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string usingStatements = "using Foo.Bar;";
                string scriptCode = 
$@"#line 1
{output}";
                string expected = GetExpected(usingStatements, string.Empty, scriptCode, string.Empty, string.Empty, string.Empty);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void LiftsClassDeclarations()
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string input = 
@"public class Foo
{
    int X { get; set; }
}

public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}

int x = 1 + 2;
Pipelines.Add(Content());

public class Baz
{
}";
                string usingStatements = "using Foo.Bar;";
                string scriptCode = 
@"#line 13

int x = 1 + 2;
Pipelines.Add(Content());
";
                string typeDeclarations =
@"#line 1
public class Foo
{
    int X { get; set; }
}

public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}
#line 16

public class Baz
{
}";
                string expected = GetExpected(usingStatements, string.Empty, scriptCode, string.Empty, typeDeclarations, string.Empty);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void LiftsUsingDirectives()
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string input =
@"using Red.Blue;
using Yellow;

public static class Foo
{
    public static string Bar(this string x) => x;
}

Pipelines.Add(Content());

public string Self(string x)
{
    return x.ToLower();
}";
                string usingStatements = "using Foo.Bar;";
                string usingDirectives = @"using Red.Blue;
using Yellow;
";

                string scriptCode =
@"#line 8

Pipelines.Add(Content());
";
                string typeDeclarations =
@"#line 3

public static class Foo
{
    public static string Bar(this string x) => x;
}
";
                string methodDeclarations =
@"#line 10

public string Self(string x)
{
    return x.ToLower();
}";
                string expected = GetExpected(usingStatements, usingDirectives, scriptCode, methodDeclarations, typeDeclarations, string.Empty);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void LiftsMethodDeclarations()
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string input =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}

Pipelines.Add(Content());

public string Self(string x)
{
    return x.ToLower();
}";
                string usingStatements = "using Foo.Bar;";
                string scriptCode =
@"#line 5

Pipelines.Add(Content());
";
                string typeDeclarations =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}
";
                string methodDeclarations =
@"#line 7

public string Self(string x)
{
    return x.ToLower();
}";
                string expected = GetExpected(usingStatements, string.Empty, scriptCode, methodDeclarations, typeDeclarations, string.Empty);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void LiftsExtensionMethodDeclarations()
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string input =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}

Pipelines.Add(Content());

public static string Self(this string x)
{
    return x.ToLower();
}";
                string usingStatements = "using Foo.Bar;";
                string scriptCode =
@"#line 5

Pipelines.Add(Content());
";
                string typeDeclarations =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}
";
                string extensionMethodDeclarations =
@"#line 7

public static string Self(this string x)
{
    return x.ToLower();
}";
                string expected = GetExpected(usingStatements, string.Empty, scriptCode, string.Empty, typeDeclarations, extensionMethodDeclarations);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }

            [Test]
            public void LiftsCommentsWithDeclarations()
            {
                // Given
                ScriptManager scriptManager = new ScriptManager();
                HashSet<Type> moduleTypes = new HashSet<Type> { typeof(Content) };
                string[] namespaces = { "Foo.Bar" };
                string input =
@"// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}

// 123
Pipelines.Add(Content());

// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}";
                string usingStatements = "using Foo.Bar;";
                string scriptCode =
@"#line 7

// 123
Pipelines.Add(Content());
";
                string typeDeclarations =
@"#line 1
// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}
";
                string methodDeclarations =
@"#line 10

// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}";
                string expected = GetExpected(usingStatements, string.Empty, scriptCode, methodDeclarations, typeDeclarations, string.Empty);

                // When
                string actual = scriptManager.Parse(input, moduleTypes, namespaces);

                // Then
                Assert.AreEqual(expected, actual);
            }
            
            // Assumes just the Content module is used
            private string GetExpected(string usingStatements, string usingDirectives, string scriptCode, string methodDeclarations, string typeDeclarations, string extensionMethodDeclarations)
            {
                return
                $@"// Generated: bring all module namespaces in scope
                {usingStatements}

                // Input: using directives
                {usingDirectives}

                public class {ScriptManager.ScriptClassName} : ScriptBase
                {{
                    public {ScriptManager.ScriptClassName}(Engine engine) : base(engine) {{ }}

                    public override void Run()
                    {{
                        // Input: script code
{scriptCode}
                    }}

                    // Input: lifted methods
{methodDeclarations}

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
                    }} 
                }}

                // Input: lifted object declarations
{typeDeclarations}

                public static class ScriptExtensionMethods
                {{
                    // Input: lifted extension methods
{extensionMethodDeclarations}
                }}";
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
