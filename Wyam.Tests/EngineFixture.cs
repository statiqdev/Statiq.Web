using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;


namespace Wyam.Tests
{
    [TestFixture]
    public class EngineFixture
    {
        [Test]
        public void ConfigureSetsPrimitiveMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Metadata.TestString = ""teststring"";
                Metadata.TestInt = 1234;
                Metadata.TestFloat = 1234.567;
                Metadata.TestBool = true;
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual("teststring", engine.Metadata.TestString);
            Assert.AreEqual(1234, engine.Metadata.TestInt);
            Assert.AreEqual(1234.567, engine.Metadata.TestFloat);
            Assert.AreEqual(true, engine.Metadata.TestBool);
        }

        [Test]
        public void ConfigureSetsAnonymousObjectMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Metadata.TestAnonymous = new { A = 1, B = ""b"" };
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, engine.Metadata.TestAnonymous.A);
            Assert.AreEqual("b", engine.Metadata.TestAnonymous.B);
        }

        // TODO: Replace with more specific tests
        [Test]
        public void ConfigurePipeline()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Pipelines.Add(
	                new ReadFile(m => m.InputPath + @""\*.cshtml""),
	                new Razor(),
	                new WriteFile(m => string.Format(@""{0}\{1}.html"", Path.GetRelativePath(m.InputPath, m.FilePath), m.FileBase)));
            ";

            // When
            engine.Configure(configScript);

            // Then
        }
    }
}
