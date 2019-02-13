using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Testing;
using Wyam.Common.Util;

namespace Wyam.Common.Tests.Util
{
    [TestFixture]
    public class CollectionExtensionsFixture : BaseFixture
    {
        public class RequireKeysTests : CollectionExtensionsFixture
        {
            [Test]
            public void ThrowsForMissingKeys()
            {
                // Given
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    { "A", "1" },
                    { "B", "2" }
                };

                // When, Then
                Should.Throw<ArgumentException>(() => dictionary.RequireKeys("A", "C"));
            }

            [Test]
            public void DoesNotThrowForPresentKeys()
            {
                // Given
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    { "A", "1" },
                    { "B", "2" },
                    { "C", "3" }
                };

                // When, Then
                Should.NotThrow(() => dictionary.RequireKeys("A", "B"));
            }

            [Test]
            public void UsesUnderlyingComparer()
            {
                // Given
                Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "A", "1" },
                    { "B", "2" },
                    { "C", "3" }
                };

                // When, Then
                Should.NotThrow(() => dictionary.RequireKeys("a", "b"));
            }
        }
    }
}
