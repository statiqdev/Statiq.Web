using NUnit.Framework;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ExecuteFixture : TraceListenerFixture
    {
        [Test]
        public void ExecuteDoesNotThrowForNullResultWithDocumentConfig()
        {
            // Given
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
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
            engine.CleanOutputFolderOnExecute = false;
            Execute execute = new Execute((c) => null);
            engine.Pipelines.Add(execute);

            // When
            engine.Execute();

            // Then
        }

    }
}
