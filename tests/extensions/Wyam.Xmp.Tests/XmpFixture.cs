using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Xmp.Tests
{
    [TestFixture]
    public class XmpFixture : BaseFixture
    {
        public class ExecuteTests : XmpFixture
        {
            [Test]
            public void ReadMetadata()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                TestExecutionContext context = new TestExecutionContext();
                IEnumerable<IDocument> documents = GetDocuments(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                Xmp directoryMetadata = new Xmp()
                    .WithMetadata("xmpRights:UsageTerms", "Copyright");

                // When
                List<IDocument> results = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Copyright"].ToString()
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            [Test]
            public void SkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                ThrowOnTraceEventType(TraceEventType.Error);
                TestExecutionContext context = new TestExecutionContext();
                IEnumerable<IDocument> documents = GetDocuments(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: true)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                List<IDocument> results = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public void DontSkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                ThrowOnTraceEventType(TraceEventType.Error);
                TestExecutionContext context = new TestExecutionContext();
                IEnumerable<IDocument> documents = GetDocuments(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                Xmp directoryMetadata = new Xmp(skipElementOnMissingMandatoryData: false)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);

                // When
                List<IDocument> results = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public void UsingNonDefaultNamespace()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                TestExecutionContext context = new TestExecutionContext();
                IEnumerable<IDocument> documents = GetDocuments(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                Xmp directoryMetadata = new Xmp()
                    .WithNamespace("http://ns.adobe.com/xap/1.0/rights/", "bla")
                    .WithMetadata("bla:UsageTerms", "Copyright");

                // When
                List<IDocument> results = directoryMetadata.Execute(new List<IDocument>(documents), context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Copyright"].ToString()
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            private IEnumerable<IDocument> GetDocuments(params string[] pathArray) =>
                pathArray.Select(x => new TestDocument(File.OpenRead(x))
                {
                    Source = x
                });
        }
    }
}