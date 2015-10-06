using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpSyntaxFixture : AnalyzeCSharpFixtureBase
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"private void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"[Foo]
[Bar]
internal class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"[Foo]
[Bar]
private int Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"[Foo]
[Bar(5)]
internal class Green : Blue", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal abstract class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal sealed class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal static class Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"private static void Blue()", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green<out TKey, TValue>", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green<out TKey, TValue> where TValue : class", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green<out TKey, TValue> : Blue, IFoo where TValue : class", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal enum Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal enum Green", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public int Blue { get; }", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public int Blue { get; set; }", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) 
    where TKey : class", GetMember(results, "Green", "Blue")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, 
    IFooBar, ICircle, ISquare, IRectangle
    where TKey : class", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
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
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual(@"internal class Green : IFoo, IBar, IFooBar", GetResult(results, "Green")["Syntax"]);
            stream.Dispose();
        }
    }
}
