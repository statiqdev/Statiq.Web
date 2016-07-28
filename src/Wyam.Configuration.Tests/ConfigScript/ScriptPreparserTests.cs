using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Configuration.ConfigScript;
using Wyam.Configuration.Preprocessing;
using Wyam.Testing;

namespace Wyam.Configuration.Tests.ConfigScript
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ScriptPreparserTests : BaseFixture
    {
        public class ParseMethodTests : ScriptManagerTests
        {
            [Test]
            public void ReturnsSameCode()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                ScriptPreparser scriptPreparser = new ScriptPreparser(preprocessor);
                string configScript = @"A
B
C";

                // When
                scriptPreparser.Parse(configScript);

                // Then
                CollectionAssert.IsEmpty(scriptPreparser.DirectiveValues);
                Assert.AreEqual(@"A
B
C", scriptPreparser.Code);
            }

            [Test]
            public void ReturnsValidDirectivesAndCommentsAllDirectives()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                ScriptPreparser scriptPreparser = new ScriptPreparser(preprocessor);
                string configScript = @"#valid a b c
#invalid a b c
#validx y z
A=
=B
#valid   x y z  
C";

                // When
                scriptPreparser.Parse(configScript);

                // Then
                Assert.AreEqual(@"//#valid a b c
//#invalid a b c
//#validx y z
A=
=B
//#valid   x y z  
C", scriptPreparser.Code);
                CollectionAssert.AreEqual(new[]
                {
                    Tuple.Create((int?)1, "valid", "a b c"),
                    Tuple.Create((int?)6, "valid", "x y z")
                }, scriptPreparser.DirectiveValues.Select(x => Tuple.Create(x.Line, x.Name, x.Value)));
            }

            [Test]
            public void DoesNotProcessDirectivesWithSpaceAfterHash()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                ScriptPreparser scriptPreparser = new ScriptPreparser(preprocessor);
                string configScript = @"# valid a b c
            A";

                // When
                scriptPreparser.Parse(configScript);

                // Then
                Assert.AreEqual(@"//# valid a b c
            A", scriptPreparser.Code);
                CollectionAssert.IsEmpty(scriptPreparser.DirectiveValues);
            }
        }

        private class TestPreprocessor : IPreprocessor
        {
            public bool ContainsDirective(string name)
            {
                return name == "valid";
            }

            public IEnumerable<IDirective> Directives
            {
                get { throw new NotImplementedException(); }
            }

            public void AddValue(DirectiveValue value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
