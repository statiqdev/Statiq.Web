using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Metadata;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class FileNameFixture
    {
        [TestCase("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:/?#[]@!$&'()*+,;=",
			"abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789")]
        [TestCase("Děku.jemeविकीвики-движка", "děkujemeविकीвикидвижка")]
        [TestCase("this is my title - and some \t\t\t\t\n   clever; (piece) of text here: [ok].", 
            "this-is-my-title-and-some-clever-piece-of-text-here-ok")]
        [TestCase("this is my title?!! /r/science/ and #firstworldproblems :* :sadface=true",
            "this-is-my-title-rscience-and-firstworldproblems-sadfacetrue")]
        [TestCase("one-two-three--four--five and a six--seven--eight-nine------ten",
            "onetwothreefourfive-and-a-sixseveneightnineten")]
        public void FileNameIsConvertedCorrectly(string input, string output)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(engine, pipeline).Clone(new []
            {
                new MetadataItem("SourceFileName", input)
            }) };
            FileName fileName = new FileName();

            // When
            IEnumerable<IDocument> documents = fileName.Execute(inputs, context);

            // Then
            Assert.AreEqual(output, documents.First()["WriteFileName"]);
        }

		[Test]
		public void FileNameShouldBeLowercase()
		{
			// Given
			string input = "FileName With MiXeD CapS";
			string output = "filename-with-mixed-caps";

            Engine engine = new Engine();
			engine.Trace.AddListener(new TestTraceListener());
			Pipeline pipeline = new Pipeline("Pipeline", engine, null);
			IExecutionContext context = new ExecutionContext(engine, pipeline);
			IDocument[] inputs = { new Document(engine, pipeline).Clone(new []
			{
                new MetadataItem("SourceFileName", input)
			}) };
			FileName fileName = new FileName();

			// When
			IEnumerable<IDocument> documents = fileName.Execute(inputs, context);

			// Then
			Assert.AreEqual(output, documents.First()["WriteFileName"]);
		}

		[Test]
		public void WithAllowedCharactersDoesNotReplaceProvidedCharacters()
		{
			// Given
			string input = "this-is-a-.net-tag";
			string output = "this-is-a-.net-tag";

			Engine engine = new Engine();
			engine.Trace.AddListener(new TestTraceListener());
			Pipeline pipeline = new Pipeline("Pipeline", engine, null);
			IExecutionContext context = new ExecutionContext(engine, pipeline);
			IDocument[] inputs = { new Document(engine, pipeline).Clone(new []
			{
                new MetadataItem("SourceFileName", input)
			}) };
			FileName fileName = new FileName();

			// When
			fileName = fileName.WithAllowedCharacters(new string[] { "-", "." });
			IEnumerable<IDocument> documents = fileName.Execute(inputs, context);

			// Then
			Assert.AreEqual(output, documents.First()["WriteFileName"]);
		}

		public static string[] ReservedChars => FileName.ReservedChars;

        [Test]
        [TestCaseSource(nameof(ReservedChars))]
        public void FileNameIsConvertedCorrectlyWithReservedChar(string character)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            string manyCharactersWow = new String(character[0], 10);
            IDocument[] inputs = { new Document(engine, pipeline).Clone(new []
            {
                new MetadataItem("SourceFileName", 
                    string.Format("testing {0} some of {0} these {0}", manyCharactersWow))
            }) };
            FileName fileName = new FileName();

            // When
            IEnumerable<IDocument> documents = fileName.Execute(inputs, context);

            // Then
            Assert.AreEqual("testing-some-of-these-", documents.First()["WriteFileName"]);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void IgnoresNullOrWhiteSpaceStrings(string input)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(engine, pipeline).Clone(new []
            {
                new MetadataItem("SourceFileName", input)
            }) };
            FileName fileName = new FileName();

            // When
            IEnumerable<IDocument> documents = fileName.Execute(inputs, context);

            // Then
            Assert.IsFalse(documents.First().ContainsKey("WriteFileName"));
        }
    }
}
