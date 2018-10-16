using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using Shouldly;
using Wyam.Hosting.Middleware;

namespace Wyam.Hosting.Tests.Middleware
{
    [TestFixture]
    public class DisableCacheMiddlewareTests
    {
        [Test]
        public async Task AddsCacheHeaders()
        {
            // Given
            TestServer server = GetServer();

            // When
            HttpResponseMessage response = await server.CreateClient().GetAsync("/");

            // Then
            response.Headers.GetValues("Cache-Control")
                .SelectMany(x => x.Split(','))
                .Select(x => x.Trim())
                .ShouldBe(new[] { "no-cache", "no-store", "must-revalidate" }, true);
            response.Headers.GetValues("Pragma").ShouldContain("no-cache");
            response.Content.Headers.GetValues("Expires").ShouldContain("0");
        }

        private TestServer GetServer() => new TestServer(
            new WebHostBuilder()
                .Configure(builder => builder.UseDisableCache()));
    }
}