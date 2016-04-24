using System.Collections.Generic;
using NUnit.Framework;
using Wyam.Configuration.Preprocessing;
using Wyam.Testing;

namespace Wyam.Configuration.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ConfigParserTests : BaseFixture
    {
        public class ParseMethodTests : ConfigCompilationTests
        {
            [Test]
            public void ReturnsConfigWithoutDelimiter()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A=
=B
C";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.IsNull(result.Declarations);
                Assert.AreEqual(@"#line 1
A=
=B
C", result.Body);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiter()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A=
=B
===
=C
D
---
-E
F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B
===
=C
D", result.Declarations);
                Assert.AreEqual(@"#line 7
-E
F", result.Body);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithTrailingSpaces()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A=
=B
===  
=C
D
---   
E
=F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B
===  
=C
D", result.Declarations);
                Assert.AreEqual(@"#line 7
E
=F", result.Body);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithLeadingSpaces()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A=
=B
    ===
=C
D
    ---
-E
F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.IsNull(result.Declarations);
                Assert.AreEqual(@"#line 1
A=
=B
    ===
=C
D
    ---
-E
F", result.Body);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithExtraLines()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A-
=B
-C
D

---

E-
-F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.AreEqual(@"#line 1
A-
=B
-C
D
", result.Declarations);
                Assert.AreEqual(@"#line 7

E-
-F", result.Body);
            }

            [Test]
            public void ReturnsDeclarations()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"A=
=B
C
---
E-
-F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B
C", result.Declarations);
            Assert.AreEqual(@"#line 5
E-
-F", result.Body);
            }

            [Test]
            public void ReturnsValidDirectives()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"#valid a b c
#invalid a b c
#validx y z
A=
=B
#valid   x y z  
C";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.IsNull(result.Declarations);
                Assert.AreEqual(@"#line 1
//#valid a b c
#invalid a b c
#validx y z
A=
=B
//#valid   x y z  
C", result.Body);
                CollectionAssert.AreEqual(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valid", "a b c"),
                    new KeyValuePair<string, string>("valid", "x y z")
                }, result.Directives);
            }

            [Test]
            public void DoesNotProcessDirectivesWithSpaceAfterHash()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"# valid a b c
A";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.IsNull(result.Declarations);
                Assert.AreEqual(@"#line 1
# valid a b c
A", result.Body);
                CollectionAssert.IsEmpty(result.Directives);
            }

            [Test]
            public void ReturnsValidDirectivesWithDeclarations()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                string configScript = @"#valid a
A=
=B
#invalid b
C
---
E-
#valid c
-F";

                // When
                ConfigParserResult result = ConfigParser.Parse(configScript, preprocessor);

                // Then
                Assert.AreEqual(@"#line 1
//#valid a
A=
=B
#invalid b
C", result.Declarations);
                Assert.AreEqual(@"#line 7
E-
//#valid c
-F", result.Body);
                CollectionAssert.AreEqual(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("valid", "a"),
                    new KeyValuePair<string, string>("valid", "c")
                }, result.Directives);
            }
        }

        private class TestPreprocessor : IPreprocessor
        {
            public bool ContainsDirective(string name)
            {
                return name == "valid";
            }
        }
    }
}
