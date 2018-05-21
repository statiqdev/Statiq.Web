using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Wyam.Hosting.Tests.Middleware
{
    [TestFixture]
    public class DisableCachingMiddlewareTests
    {
        private readonly TestServer _host;

        public DisableCachingMiddlewareTests()
        {
            _host = new TestServer(new WebHostBuilder().UseStartup<Startup>().ConfigureServices(services =>
                services
                    .WithDefaultExtensions(new DefaultExtensionsOptions())
                    .WithServerOptions(new PreviewServerOptions())));
        }

        [Test]
        public async Task WhenDisableCache()
        {
            HttpResponseMessage response = await _host.CreateClient().GetAsync("/");
            string body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(false);
        }
    }
}