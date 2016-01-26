using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration;
using Wyam.Testing;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ConfigSplitterTests : BaseFixture
    {
        public class SplitMethodTests : ConfigScriptTests
        {
            [Test]
            public void ReturnsBothPartsWithDelimiter()
            {
                // Given
                string configScript = @"A=
=B
===
=C
D";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B", configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 4
=C
D", configParts.Config);
            }

            [Test]
            public void ReturnsBothPartsWithDelimiterWithTrailingSpaces()
            {
                // Given
                string configScript = @"A=
=B
===  
=C
D";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B", configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 4
=C
D", configParts.Config);
            }

            [Test]
            public void ReturnsConfigWithDelimiterWithLeadingSpaces()
            {
                // Given
                string configScript = @"A=
=B
    ===
=C
D";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.IsNull(configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 1
A=
=B
    ===
=C
D", configParts.Config);
            }

            [Test]
            public void ReturnsBothPartsWithDelimiterWithExtraLines()
            {
                // Given
                string configScript = @"A=
=B

===

=C
D";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B
", configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 5

=C
D", configParts.Config);
            }

            [Test]
            public void ReturnsConfigWithoutDelimiter()
            {
                // Given
                string configScript = @"A=
=B
C";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.IsNull(configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 1
A=
=B
C", configParts.Config);

            }





            [Test]
            public void ReturnsDeclarationsWithDelimiter()
            {
                // Given
                string configScript = @"A=
=B
===
=C
D
---
-E
F";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B", configParts.Setup);
                Assert.AreEqual(@"#line 4
=C
D", configParts.Declarations);
                Assert.AreEqual(@"#line 7
-E
F", configParts.Config);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithTrailingSpaces()
            {
                // Given
                string configScript = @"A=
=B
===  
=C
D
---   
E
=F";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B", configParts.Setup);
                Assert.AreEqual(@"#line 4
=C
D", configParts.Declarations);
                Assert.AreEqual(@"#line 7
E
=F", configParts.Config);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithLeadingSpaces()
            {
                // Given
                string configScript = @"A=
=B
    ===
=C
D
    ---
-E
F";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.IsNull(configParts.Setup);
                Assert.IsNull(configParts.Declarations);
                Assert.AreEqual(@"#line 1
A=
=B
    ===
=C
D
    ---
-E
F", configParts.Config);
            }

            [Test]
            public void ReturnsDeclarationsWithDelimiterWithExtraLines()
            {
                // Given
                string configScript = @"A=
=B

===

=C
D

---

E-
-F";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.AreEqual(@"#line 1
A=
=B
", configParts.Setup);
                Assert.AreEqual(@"#line 5

=C
D
", configParts.Declarations);
                Assert.AreEqual(@"#line 10

E-
-F", configParts.Config);
            }

            [Test]
            public void ReturnDeclarationsWithoutSetup()
            {
                // Given
                string configScript = @"A=
=B
C
---
E-
-F";

                // When
                ConfigParts configParts = ConfigSplitter.Split(configScript);

                // Then
                Assert.IsNull(configParts.Setup);
                Assert.AreEqual(@"#line 1
A=
=B
C", configParts.Declarations);
            Assert.AreEqual(@"#line 5
E-
-F", configParts.Config);

            }
        }
    }
}
