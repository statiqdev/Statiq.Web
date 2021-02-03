using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Shortcodes;

namespace Statiq.Web.Tests.Shortcodes
{
    [TestFixture]
    public class EmbedShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : EmbedShortcodeFixture
        {
            [Test]
            public async Task IssuesRequestToCorrectEndpoint()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                string requestUri = null;
                context.HttpResponseFunc = (request, __) =>
                {
                    requestUri = request.RequestUri.ToString();
                    HttpResponseMessage response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new System.Net.Http.StringContent(@"{""html"": ""<div></div>""}"),
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypes.Html);
                    return response;
                };
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("endpoint", "https://foo.bar/embed"),
                    new KeyValuePair<string, string>("url", "https://foo.bar?123"),
                    new KeyValuePair<string, string>("format", "html"),
                    new KeyValuePair<string, string>("maxwidth", "300"),
                    new KeyValuePair<string, string>("maxheight", "400"),
                };
                EmbedShortcode embed = new EmbedShortcode();

                // When
                await embed.ExecuteAsync(args, document, context);

                // Then
                requestUri.ShouldBe("https://foo.bar/embed?url=https%3A%2F%2Ffoo.bar%3F123&format=html&maxwidth=300&maxheight=400");
            }
        }
    }
}
