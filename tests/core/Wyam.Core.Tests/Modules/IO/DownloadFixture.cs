using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [NonParallelizable]
    public class DownloadFixture : BaseFixture
    {
        public class ExecuteTests : DownloadFixture
        {
            [Test]
            public void SingleHtmlDownloadGetStream()
            {
                // Given
                IDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/");

                // When
                IList<IDocument> results = download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                IDocument result = results.Single();
                Assert.IsNotNull(result.Source, "Source cannot be empty");

                Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                Assert.IsNotNull(headers, "Header cannot be null");
                Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                foreach (KeyValuePair<string, string> h in headers)
                {
                    Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                    Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                }

                using (Stream stream = result.GetStream())
                {
                    string content = new StreamReader(stream).ReadToEnd();
                    Assert.IsNotEmpty(content, "Download cannot be empty");
                }
            }

            [Test]
            public void MultipleHtmlDownload()
            {
                // Given
                IDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/", "https://github.com/Wyamio/Wyam");

                // When
                IList<IDocument> results = download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                foreach (IDocument result in results)
                {
                    Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                    Assert.IsNotNull(headers, "Header cannot be null");
                    Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                    foreach (KeyValuePair<string, string> h in headers)
                    {
                        Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                        Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                    }

                    using (Stream stream = result.GetStream())
                    {
                        string content = new StreamReader(stream).ReadToEnd();
                        Assert.IsNotEmpty(content, "Download cannot be empty");
                    }
                }
            }

            [Test]
            public void SingleImageDownload()
            {
                // Given
                IDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                IModule download = new Download().WithUris("https://wyam.io/assets/img/logo.png");

                // When
                IList<IDocument> results = download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                using (Stream stream = results.Single().GetStream())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }

            [Test]
            public void SingleImageDownloadWithRequestHeader()
            {
                // Given
                IDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                RequestHeaders header = new RequestHeaders();
                header.Accept.Add("image/jpeg");
                IModule download = new Download().WithUri("https://wyam.io/assets/img/logo.png", header);

                // When
                IList<IDocument> results = download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                using (Stream stream = results.Single().GetStream())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }
        }
    }
}
