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
    public class DirectiveParserFixture : BaseFixture
    {
        public class ParseTests : ScriptManagerFixture
        {
            [Test]
            public void ReturnsSameCode()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                DirectiveParser directiveParser = new DirectiveParser(preprocessor);
                string configScript = @"A
B
C";

                // When
                directiveParser.Parse(configScript);

                // Then
                CollectionAssert.IsEmpty(directiveParser.DirectiveValues);
                Assert.AreEqual(@"A
B
C", directiveParser.Code);
            }

            [Test]
            public void ReturnsAndCommentsValidDirectives()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                DirectiveParser directiveParser = new DirectiveParser(preprocessor);
                string configScript = @"#valid a b c
#invalid a b c
#validx y z
A=
=B
#valid   x y z  
C";

                // When
                directiveParser.Parse(configScript);

                // Then
                Assert.AreEqual(@"//#valid a b c
#invalid a b c
#validx y z
A=
=B
//#valid   x y z  
C", directiveParser.Code);
                CollectionAssert.AreEqual(new[]
                {
                    Tuple.Create((int?)1, "valid", "a b c"),
                    Tuple.Create((int?)6, "valid", "x y z")
                }, directiveParser.DirectiveValues.Select(x => Tuple.Create(x.Line, x.Name, x.Value)));
            }

            [Test]
            public void DoesNotProcessDirectivesWithSpaceAfterHash()
            {
                // Given
                IPreprocessor preprocessor = new TestPreprocessor();
                DirectiveParser directiveParser = new DirectiveParser(preprocessor);
                string configScript = @"# valid a b c
            A";

                // When
                directiveParser.Parse(configScript);

                // Then
                Assert.AreEqual(@"# valid a b c
            A", directiveParser.Code);
                CollectionAssert.IsEmpty(directiveParser.DirectiveValues);
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
