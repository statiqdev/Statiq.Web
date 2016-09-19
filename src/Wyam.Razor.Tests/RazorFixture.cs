using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Razor.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class RazorFixture : BaseFixture
    {
        public class ExecuteTests : RazorFixture
        {
            [Test]
            public void SimpleTemplate()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
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
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
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
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
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
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(new FilePath("C:/Temp/temp.txt"));
                document.GetStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes(@"<p>@Document.Source</p>")));
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, @"<p>file:///C:/Temp/temp.txt</p>");
            }

            [Test]
            public void DocumentAsModel()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument("C:/Temp/temp.txt", @"<p>@Model.Source</p>");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, @"<p>file:///C:/Temp/temp.txt</p>");  // The slash prefix is just added by our dumb mock
            }

            [Test]
            public void LoadLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"Layout/Test.cshtml", 
@"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, 
@"LAYOUT
<p>This is a test</p>");
            }

            [Test]
            public void RenderModuleDefinedLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"Layout/Test.cshtml",
@"<p>This is a test</p>");
                Razor razor = new Razor().WithLayout("_Layout.cshtml");

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT
<p>This is a test</p>");
            }

            [Test]
            public void LoadViewStartAndLayoutFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"ViewStartAndLayout/Test.cshtml",
@"<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT2
<p>This is a test</p>");
            }

            [Test]
            public void AlternateViewStartPath()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"AlternateViewStartPath/Test.cshtml",
@"<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart(@"/AlternateViewStart/_ViewStart.cshtml");

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT3
<p>This is a test</p>");
            }

            [Test]
            public void AlternateViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"AlternateViewStartPath/Test.cshtml",
@"<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart(@"/AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT3
<p>This is a test</p>");
            }

            [Test]
            public void AlternateRelativeViewStartPathWithRelativeLayout()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"AlternateViewStartPath/Test.cshtml",
@"<p>This is a test</p>");
                Razor razor = new Razor().WithViewStart(@"AlternateViewStart/_ViewStartRelativeLayout.cshtml");

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT3
<p>This is a test</p>");
            }

            [Test]
            public void IgnoresUnderscoresByDefault()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document1 = GetDocument(@"IgnoreUnderscores/Test.cshtml",
@"@{
	Layout = ""_Layout.cshtml"";
}
<p>This is a test</p>");
                IDocument document2 = GetDocument(@"IgnoreUnderscores/_Layout.cshtml",
@"LAYOUT4
@RenderBody()");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document1, document2 }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document1,
@"LAYOUT4
<p>This is a test</p>");
            }

            [Test]
            public void AlternateIgnorePrefix()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document1 = GetDocument(@"AlternateIgnorePrefix/Test.cshtml",
@"<p>This is a test</p>");
                IDocument document2 = GetDocument(@"AlternateIgnorePrefix/IgnoreMe.cshtml",
@"<p>Ignore me</p>");
                Razor razor = new Razor().IgnorePrefix("Ignore");

                // When
                razor.Execute(new[] { document1, document2 }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document1,
@"<p>This is a test</p>");
            }

            [Test]
            public void RenderLayoutSection()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"LayoutWithSection/Test.cshtml",
@"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>");
            }

            [Test]
            public void RenderLayoutSectionOnMultipleExecution()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument document = GetDocument(@"LayoutWithSection/Test.cshtml",
@"@{
	Layout = ""_Layout.cshtml"";
}
@section MySection {
<p>Section Content</p>
}
<p>This is a test</p>");
                Razor razor = new Razor();

                // When
                razor.Execute(new[] { document }, context).ToList();
                razor.Execute(new[] { document }, context).ToList();

                // Then
                context.Received(2).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document,
@"LAYOUT5

<p>Section Content</p>

<p>This is a test</p>");
            }

            private IDocument GetDocument(string source, string content)
            {
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(new FilePath("/" + source));
                document.ContainsKey(Keys.RelativeFilePath).Returns(true);
                document.String(Keys.RelativeFilePath).Returns(source);
                document.ContainsKey(Keys.SourceFileName).Returns(true);
                document.FilePath(Keys.SourceFileName).Returns(new FilePath(source).FileName);
                document.GetStream().Returns(
                    new MemoryStream(Encoding.UTF8.GetBytes(content)),
                    new MemoryStream(Encoding.UTF8.GetBytes(content)));  // Return a new memory stream if called again
                return document;
            }

            private IExecutionContext GetExecutionContext(Engine engine)
            {
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.Namespaces.Returns(engine.Namespaces);
                IReadOnlyFileSystem fileSystem = GetFileSystem();
                context.FileSystem.Returns(fileSystem);
                FilePath result;
                context.TryConvert(Arg.Any<object>(), out result).Returns(x =>
                {
                    x[1] = (FilePath) x[0];
                    return true;
                });
                return context;
            }

            private IReadOnlyFileSystem GetFileSystem()
            {
                IReadOnlyFileSystem fileSystem = Substitute.For<IReadOnlyFileSystem>();
                IFileProvider fileProvider = GetFileProvider();
                fileSystem.GetInputFile(Arg.Any<FilePath>()).Returns(x =>
                {
                    FilePath path = x.ArgAt<FilePath>(0);
                    if (!path.IsAbsolute)
                    {
                        path = new FilePath("/" + path.FullPath);
                    }
                    return fileProvider.GetFile(path);
                });
                fileSystem.GetInputDirectory(Arg.Any<DirectoryPath>()).Returns(x => fileProvider.GetDirectory(x.ArgAt<DirectoryPath>(0)));
                fileSystem.RootPath.Returns(new DirectoryPath("/"));
                return fileSystem;
            }

            private IFileProvider GetFileProvider()
            {
                TestFileProvider fileProvider = new TestFileProvider();

                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/AlternateIgnorePrefix");
                fileProvider.AddDirectory("/AlternateViewStart");
                fileProvider.AddDirectory("/AlternateViewStartPath");
                fileProvider.AddDirectory("/IgnoreUnderscores");
                fileProvider.AddDirectory("/Layout");
                fileProvider.AddDirectory("/LayoutWithSection");
                fileProvider.AddDirectory("/SimpleTemplate");
                fileProvider.AddDirectory("/ViewStartAndLayout");

                fileProvider.AddFile("/Layout/_Layout.cshtml",
@"LAYOUT
@RenderBody()");
                fileProvider.AddFile("/ViewStartAndLayout/_ViewStart.cshtml",
@"@{
	Layout = ""_Layout.cshtml"";
}");
                fileProvider.AddFile("/ViewStartAndLayout/_Layout.cshtml",
@"LAYOUT2
@RenderBody()");
                fileProvider.AddFile("/AlternateViewStart/_ViewStart.cshtml",
@"@{
	Layout = @""/AlternateViewStart/_Layout.cshtml"";
}");
                fileProvider.AddFile("/AlternateViewStart/_ViewStartRelativeLayout.cshtml",
@"@{
	Layout = @""_Layout.cshtml"";
}");
                fileProvider.AddFile("/AlternateViewStart/_Layout.cshtml",
@"LAYOUT3
@RenderBody()");
                fileProvider.AddFile("/IgnoreUnderscores/_Layout.cshtml",
@"LAYOUT4
@RenderBody()");
                fileProvider.AddFile("/LayoutWithSection/_Layout.cshtml",
@"LAYOUT5
@RenderSection(""MySection"", false)
@RenderBody()");

                return fileProvider;
            }
        }
    }
}