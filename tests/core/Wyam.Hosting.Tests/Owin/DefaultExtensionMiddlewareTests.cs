using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Wyam.Hosting.Tests.Owin
{
    [TestFixture]
    public class DefaultExtensionMiddlewareTests
    {
        private readonly TestServer _host;

        public DefaultExtensionMiddlewareTests()
        {
            _host = new TestServer(new WebHostBuilder().UseStartup<Startup>().ConfigureServices(services =>
                services
                    .WithDefaultExtensions(new DefaultExtensionsOptions())
                    .WithServerOptions(new PreviewServerOptions())));
        }

        [Test]
        public async Task WhenExtensionsProvided()
        {
            HttpResponseMessage response = await _host.CreateClient().GetAsync("/");
            string body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(false);
        }
    }
}