using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
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
                string code = @"
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
                Assert.AreEqual(@"internal class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void ImplicitMemberAccessibility()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"private void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void ExplicitClassAccessibility()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void ExplicitMemberAccessibility()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void ClassAttributes()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        [  Foo]
                        [Bar  ]
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
                Assert.AreEqual(@"[Foo]
[Bar]
internal class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void MethodAttributes()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"[Foo]
[Bar]
private int Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void ClassComments()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"[Foo]
[Bar(5)]
internal class Green : Blue", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void AbstractClass()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal abstract class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void SealedClass()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal sealed class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void StaticClass()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal static class Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void StaticMethod()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"private static void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void ClassWithGenericTypeParameters()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal class Green<out TKey, TValue>", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void ClassWithGenericTypeParametersAndConstraints()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal class Green<out TKey, TValue> where TValue : class", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void ClassWithGenericTypeParametersAndBaseAndConstraints()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal class Green<out TKey, TValue> : Blue, IFoo where TValue : class", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void MethodWithGenericParameters()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void MethodWithGenericParametersAndConstraints()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void Enum()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal enum Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void EnumWithBase()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal enum Green", GetResult(results, "Green")["Syntax"]);
            }

            [Test]
            public void ExplicitProperty()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public int Blue { get; }", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void AutoProperty()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public int Blue { get; set; }", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void WrapsForLongMethodSignature()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) 
    where TKey : class", GetMember(results, "Green", "Blue")["Syntax"]);
            }

            [Test]
            public void WrapsForLongClassSignature()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, 
    IFooBar, ICircle, ISquare, IRectangle
    where TKey : class", GetResult(results, "Green")["Syntax"]);
            }


            [Test]
            public void ClassWithInterfaces()
            {
                // Given
                string code = @"
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
                Assert.AreEqual(@"internal class Green : IFoo, IBar, IFooBar", GetResult(results, "Green")["Syntax"]);
            }
        }
    }
}