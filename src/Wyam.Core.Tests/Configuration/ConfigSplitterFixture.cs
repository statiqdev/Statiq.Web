using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ConfigSplitterFixture
    {
        [Test]
        public void SplitReturnsBothPartsWithDelimiter()
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
        public void SplitReturnsBothPartsWithDelimiterWithTrailingSpaces()
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
        public void SplitReturnsConfigWithDelimiterWithLeadingSpaces()
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
        public void SplitReturnsBothPartsWithDelimiterWithExtraLines()
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
        public void SplitReturnsConfigWithoutDelimiter()
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
        public void SplitReturnsDeclarationsWithDelimiter()
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
        public void SplitReturnsDeclarationsWithDelimiterWithTrailingSpaces()
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
        public void SplitReturnsDeclarationsWithDelimiterWithLeadingSpaces()
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
        public void SplitReturnsDeclarationsWithDelimiterWithExtraLines()
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
        public void SplitReturnDeclarationsWithoutSetup()
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
