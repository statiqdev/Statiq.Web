using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.OpenAPI.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class OpenAPIFixture : BaseFixture
    {
        public class ExecuteTests : OpenAPIFixture
        {
            [Test]
            public void SetsMetadataKey()
            {
                //// Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(Resources.OPENAPI3DOCUMENT);
                OpenAPI openAPI = new OpenAPI("myOpenApi");

                // When
                IList<IDocument> documents = openAPI.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "myOpenApi" }, true);
            }

            [Test]
            public void GeneratesOpenApiDocumentObject()
            {
                //// Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(Resources.OPENAPI3DOCUMENT);
                OpenAPI module = new OpenAPI();

                // When
                IList<IDocument> documents = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { OpenAPI.OpenAPIDEFAULTKEY }, true);
                documents[0][OpenAPI.OpenAPIDEFAULTKEY].ShouldBeOfType<OpenApiDocument>();
                OpenApiDocument api = (OpenApiDocument)documents[0][OpenAPI.OpenAPIDEFAULTKEY];
                api.Paths.Count.ShouldBe(2);
            }

            [Test]
            [Ignore("This is a question, should a empty document result something through this module, or an exception is legit ?")]
            public void ReturnsDocumentIfEmptyInput()
            {
                //// Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(Environment.NewLine);
                OpenAPI openAPI = new OpenAPI();

                // When
                IList<IDocument> documents = openAPI.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBeEmpty();
            }
        }
    }
}