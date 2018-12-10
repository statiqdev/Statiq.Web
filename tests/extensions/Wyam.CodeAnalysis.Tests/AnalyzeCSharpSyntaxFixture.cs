using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.CodeAnalysis.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class AnalyzeCSharpSyntaxFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpSyntaxFixture
        {
            [Test]
            public void ImplicitClassAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal class Green");
            }

            [Test]
            public void ImplicitMemberAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"private void Blue()");
            }

            [Test]
            public void ExplicitClassAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"public class Green");
            }

            [Test]
            public void ExplicitMemberAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            internal void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"internal void Blue()");
            }

            [Test]
            public void ClassAttributes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        [  Foo]
                        [Bar ,Foo ]
                        class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar, Foo]
internal class Green",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void MethodAttributes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            [  Foo]
                            [Bar  ]
                            int Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar]
private int Blue()",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ClassComments()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        // asfd
                        [  Foo /* asdf */]
                        [Bar( /* asdf */ 5)  ] // asdf
                        /* asfd */
                        class /* asfd */ Green // asdf 
                            /* asdf */ : Blue  // asfd
                        {
                            // asdf
                        }

                        class Blue
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar(5)]
internal class Green : Blue",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AbstractClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        abstract class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal abstract class Green");
            }

            [Test]
            public void SealedClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        sealed class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal sealed class Green");
            }

            [Test]
            public void StaticClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        static class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal static class Green");
            }

            [Test]
            public void StaticMethod()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            static void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"private static void Blue()");
            }

            [Test]
            public void ClassWithGenericTypeParameters()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue>
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal class Green<out TKey, TValue>");
            }

            [Test]
            public void ClassWithGenericTypeParametersAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue> where TValue : class
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal class Green<out TKey, TValue> where TValue : class");
            }

            [Test]
            public void ClassWithGenericTypeParametersAndBaseAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue> : Blue, 


                            IFoo 

                    where TValue : class
                        {
                        }

                        class Blue
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal class Green<out TKey, TValue> : Blue, IFoo where TValue : class");
            }

            [Test]
            public void MethodWithGenericParameters()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)");
            }

            [Test]
            public void MethodWithGenericParametersAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class");
            }

            [Test]
            public void Enum()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        enum Green
                        {
                            Foo = 3,
                            Bar = 5
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal enum Green");
            }

            [Test]
            public void EnumWithBase()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        enum Green : long
                        {
                            Foo = 3,
                            Bar = 5
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal enum Green");
            }

            [Test]
            public void ExplicitProperty()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public int Blue
			                {
				                get { return 1 ; }
			                }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"public int Blue { get; }");
            }

            [Test]
            public void AutoProperty()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public int Blue { get; set; }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe(@"public int Blue { get; set; }");
            }

            [Test]
            public void WrapsForLongMethodSignature()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) where TKey : class
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ToString().ShouldBe(
                    @"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) 
    where TKey : class",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void WrapsForLongClassSignature()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, IFooBar, ICircle, ISquare, IRectangle where TKey : class
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"internal class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, 
    IFooBar, ICircle, ISquare, IRectangle
    where TKey : class",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ClassWithInterfaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green : IFoo, IBar, IFooBar
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe(@"internal class Green : IFoo, IBar, IFooBar");
            }
        }
    }
}