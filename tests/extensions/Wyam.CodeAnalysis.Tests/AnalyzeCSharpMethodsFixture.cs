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
    public class AnalyzeCSharpMethodsFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpMethodsFixture
        {
            [Test]
            public void ClassMembersContainsMethods()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
                            {
                            }

                            void Red()
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
                CollectionAssert.AreEquivalent(new[] { "Green", "Red" }, 
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            }

            [Test]
            public void ContainingTypeIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
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
                Assert.AreEqual("Blue", GetMember(results, "Blue", "Green").Get<IDocument>("ContainingType")["Name"]);
            }

            [Test]
            public void WritePathIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                            void X()
                            {
                            }
                        }
                    }

                    class Yellow
                    {
                        void Y<T>()
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
                CollectionAssert.AreEquivalent(new[] { "global/439037DE/66F23CDD.html", "Foo/414E2165/A94FD382.html" },
                    results.Where(x => x["Kind"].Equals("Method")).Select(x => ((FilePath)x[Keys.WritePath]).FullPath));
            }

            [Test]
            public void DisplayNameIsCorrect()
            {
                // Given
                string code = @"
                    class Yellow
                    {
                        public X()
                        {
                        }

                        void Y<T>(T a, int b)
                        {
                        }

                        Z(bool a)
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
                Assert.AreEqual("X()", GetMember(results, "Yellow", "X")["DisplayName"]);
                Assert.AreEqual("Y<T>(T, int)", GetMember(results, "Yellow", "Y")["DisplayName"]);
                Assert.AreEqual("Z(bool)", GetMember(results, "Yellow", "Z")["DisplayName"]);
            }

            [Test]
            public void ReturnTypeIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            int Green()
                            {
                                return 0;
                            }

                            Red GetRed()
                            {
                                return new Red();
                            }

                            TFoo Bar<TFoo>()
                            {
                                return default(TFoo);
                            }
                        }

                        public class Red
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
                Assert.AreEqual("int", GetMember(results, "Blue", "Green").Get<IDocument>("ReturnType")["DisplayName"]);
                Assert.AreEqual("Red", GetMember(results, "Blue", "GetRed").Get<IDocument>("ReturnType")["DisplayName"]);
                Assert.AreEqual("TFoo", GetMember(results, "Blue", "Bar").Get<IDocument>("ReturnType")["DisplayName"]);
            }

            [Test]
            public void ReturnTypeParamReferencesClass()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                            public T Blue()
                            {
                                return default(T);
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
                Assert.AreEqual("Red", GetMember(results, "Red", "Blue").Get<IDocument>("ReturnType").Get<IDocument>("DeclaringType")["Name"]);
            }

            [Test]
            public void ClassMemberExcludedByPredicate()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
                            {
                            }

                            void Red()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WhereSymbol(x => x.Name != "Green");

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Blue", "Red" }, results.Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new[] { "Red" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Where(x => x.Get<bool>("IsResult")).Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new[] { "Red", "Green" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            }
        }
    }
}
