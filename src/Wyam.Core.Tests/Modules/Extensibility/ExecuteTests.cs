using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ExecuteTests : BaseFixture
    {
        public class ExecuteMethodTests : ExecuteTests
        {
            [Test]
            public void ExecuteDoesNotThrowForNullResultWithDocumentConfig()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                Execute execute = new Execute((d, c) => null);
                engine.Pipelines.Add(execute);

                // When
                engine.Execute();

                // Then
            }

            [Test]
            public void ExecuteDoesNotThrowForNullResultWithContextConfig()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                Execute execute = new Execute(c => null);
                engine.Pipelines.Add(execute);

                // When
                engine.Execute();

                // Then
            }

            [Test]
            public void ExecuteDoesNotRequireReturnValueForDocumentConfig()
            {
                // Given
                int a = 0;
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                Execute execute = new Execute((d, c) => { a = a + 1; });
                engine.Pipelines.Add(execute);

                // When
                engine.Execute();

                // Then
            }

            [Test]
            public void ExecuteDoesNotRequireReturnValueForContextConfig()
            {
                // Given
                int a = 0;
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                Execute execute = new Execute(c => { a = a + 1; });
                engine.Pipelines.Add(execute);

                // When
                engine.Execute();

                // Then
            }

            [Test]
            public void ExecuteReturnsDocumentForSingleResultDocumentFromContextConfig()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                IDocument document = Substitute.For<IDocument>();
                Execute execute = new Execute(c => document);
                engine.Pipelines.Add("Test", execute);

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEquivalent(new[] { document }, engine.Documents["Test"]);
            }

            [Test]
            public void ExecuteReturnsDocumentForSingleResultDocumentFromDocumentConfig()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputPathOnExecute = false;
                IDocument document = Substitute.For<IDocument>();
                Execute execute = new Execute((d, c) => document);
                engine.Pipelines.Add("Test", execute);

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEquivalent(new[] { document }, engine.Documents["Test"]);
            }
        }
    }
}
