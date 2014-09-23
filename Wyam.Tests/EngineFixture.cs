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
                Meta.TestString = ""teststring"";
                Meta.TestInt = 1234;
                Meta.TestFloat = 1234.567;
                Meta.TestBool = true;
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual("teststring", engine.Meta.TestString);
            Assert.AreEqual(1234, engine.Meta.TestInt);
            Assert.AreEqual(1234.567, engine.Meta.TestFloat);
            Assert.AreEqual(true, engine.Meta.TestBool);
        }

        [Test]
        public void ConfigureSetsAnonymousObjectMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Meta.TestAnonymous = new { A = 1, B = ""b"" };
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, engine.Meta.TestAnonymous.A);
            Assert.AreEqual("b", engine.Meta.TestAnonymous.B);
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
