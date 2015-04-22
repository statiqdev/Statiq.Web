using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Configuration;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class ConfiguratorFixture
    {
        [Test]
        public void GetConfigPartsReturnsBothPartsWithDelimiter()
        {
            // Given
            Engine engine = new Engine();
            Configurator configurator = new Configurator(engine);
            string configScript = @"A-
-B
---
-C
D";

            // When
            Tuple<string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.AreEqual("A-" + Environment.NewLine + "-B", configParts.Item1);
            Assert.AreEqual("-C" + Environment.NewLine + "D", configParts.Item2);
        }

        [Test]
        public void GetConfigPartsReturnsConfigWithoutDelimiter()
        {
            // Given
            Engine engine = new Engine();
            Configurator configurator = new Configurator(engine);
            string configScript = @"A-
-B
C";

            // When
            Tuple<string, string> configParts = configurator.GetConfigParts(configScript);

            // Then
            Assert.IsNull(configParts.Item1);
            Assert.AreEqual("A-" + Environment.NewLine + "-B" + Environment.NewLine + "C", configParts.Item2);
            
        }
    }
}
