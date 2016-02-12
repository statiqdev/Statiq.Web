using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core;
using Wyam.Core.Modules;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;

namespace Wyam.Modules.Razor.Tests
{
    [TestFixture]
    public class RazorTests : BaseFixture
    {
        public class ExecuteMethodTests : RazorTests
        {
            [Test]
            public void SimpleTemplate()
            {
                // Given
                string inputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, @".\Input");
                if (!Directory.Exists(inputFolder))
                {
                    Directory.CreateDirectory(inputFolder);
                }
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.RootFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.InputFolder.Returns(inputFolder);
                Engine engine = new Engine();
                engine.Configure();
                context.Assemblies.Returns(engine.Assemblies);
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"@for(int c = 0 ; c < 5 ; c++) { <p>@c</p> }")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, " <p>0</p>  <p>1</p>  <p>2</p>  <p>3</p>  <p>4</p> ");
            }

            [Test]
            [Parallelizable(ParallelScope.None)]
            public void Tracing()
            {
                // Given
                string inputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, @".\Input");
                if (!Directory.Exists(inputFolder))
                {
                    Directory.CreateDirectory(inputFolder);
                }
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.RootFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.InputFolder.Returns(inputFolder);
                Engine engine = new Engine();
                engine.Configure();
                context.Assemblies.Returns(engine.Assemblies);
                context.Namespaces.Returns(engine.Namespaces);
                IDocument document = Substitute.For<IDocument>();
                TraceListener traceListener = new TraceListener();
                Trace.AddListener(traceListener);
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"@{ Trace.Information(""Test""); }")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Trace.RemoveListener(traceListener);
                CollectionAssert.Contains(traceListener.Messages, "Test");
            }

            public class TraceListener : System.Diagnostics.ConsoleTraceListener
            {
                public List<string> Messages { get; set; } = new List<string>();

                public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
                {
                    LogMessage(eventType, message);
                }

                public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
                {
                    LogMessage(eventType, string.Format(format, args));
                }

                private void LogMessage(System.Diagnostics.TraceEventType eventType, string message)
                {
                    Messages.Add(message);
                }
            }

            [Test]
            public void Metadata()
            {
                // Given
                string inputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, @".\Input");
                if (!Directory.Exists(inputFolder))
                {
                    Directory.CreateDirectory(inputFolder);
                }
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.RootFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.InputFolder.Returns(inputFolder);
                Engine engine = new Engine();
                engine.Configure();
                context.Assemblies.Returns(engine.Assemblies);
                IDocument document = Substitute.For<IDocument>();
                document["MyKey"].Returns("MyValue");
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"<p>@Metadata[""MyKey""]</p>")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, "<p>MyValue</p>");
            }

            [Test]
            public void Document()
            {
                // Given
                string inputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, @".\Input");
                if (!Directory.Exists(inputFolder))
                {
                    Directory.CreateDirectory(inputFolder);
                }
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.RootFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.InputFolder.Returns(inputFolder);
                Engine engine = new Engine();
                engine.Configure();
                context.Assemblies.Returns(engine.Assemblies);
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(@"C:\Temp\temp.txt");
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"<p>@Document.Source</p>")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, @"<p>C:\Temp\temp.txt</p>");
            }

            [Test]
            public void DocumentAsModel()
            {
                // Given
                string inputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, @".\Input");
                if (!Directory.Exists(inputFolder))
                {
                    Directory.CreateDirectory(inputFolder);
                }
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.RootFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.InputFolder.Returns(inputFolder);
                Engine engine = new Engine();
                engine.Configure();
                context.Assemblies.Returns(engine.Assemblies);
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(@"C:\Temp\temp.txt");
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"<p>@Model.Source</p>")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, @"<p>C:\Temp\temp.txt</p>");
            }

            [Test]
            public void LoadSimpleTemplateFile()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"SimpleTemplate\Test.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual(@"<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void LoadLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"Layout\Test.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void LoadViewStartAndLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"ViewStartAndLayout\Test.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void AlternateViewStartPath()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"AlternateViewStartPath\Test.cshtml");
                Razor razor = new Razor().WithViewStart(@"AlternateViewStart\_ViewStart.cshtml");
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void IgnoresUnderscoresByDefault()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"IgnoreUnderscores\*.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void AlternateIgnorePrefix()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"AlternateIgnorePrefix\*.cshtml");
                Razor razor = new Razor().IgnorePrefix("Ignore");
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual(@"<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void RenderLayoutSection()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"LayoutWithSection\Test.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n\r\n<p>Section Content</p>\r\n\r\n\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }

            [Test]
            public void RenderLayoutSectionOnMultipleExecution()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input\";
                ReadFiles readFiles = new ReadFiles(@"LayoutWithSection\Test.cshtml");
                Razor razor = new Razor();
                Meta meta = new Meta("Content", (x, y) => x.Content);
                engine.Pipelines.Add("Pipeline", readFiles, razor, meta);

                // When
                engine.Execute();
                engine.Execute();

                // Then
                Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Count());
                Assert.AreEqual("LAYOUT\r\n\r\n<p>Section Content</p>\r\n\r\n\r\n<p>This is a test</p>", engine.Documents.FromPipeline("Pipeline").First().String("Content"));
            }
        }
    }
}