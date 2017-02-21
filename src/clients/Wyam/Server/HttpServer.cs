using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Owin;

namespace Wyam.Hosting
{
    internal class HttpServer : IDisposable
    {
        private IWebHost _host;

        public void StartServer(int port, Action<IAppBuilder> owinConfiguration)
        {
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://localhost:{port}")
                .Configure(builder =>
                {
                    // Enable WebSocket support
                    builder.UseWebSockets();

                    // Support drop-in replacement
                    builder.UseOwinBuilder(owinConfiguration);
                })
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}