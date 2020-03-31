using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Statiq.Web.Hosting.Tests
{
    [TestFixture]
    public class ServerTests
    {
        private static int GetEphemeralPort()
        {
            // Based on http://stackoverflow.com/a/150974/2001966
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        [Test]
        public void ServerShouldBindOnConstructionWithoutUrlReservations()
        {
            // Given
            int port = GetEphemeralPort();
            Server server = null;

            // When, Then
            Should.NotThrow(() => server = new Server(TestContext.CurrentContext.TestDirectory, port));
            server?.Dispose();
        }

        [Test]
        public void ServerShouldBindOnStartWithoutUrlReservations()
        {
            // Given
            int port = GetEphemeralPort();
            Server server = new Server(TestContext.CurrentContext.TestDirectory, port);

            // When, Then
            Should.NotThrow(() => server.StartAsync());
            server.Dispose();
        }

        [Test]
        public async Task ServerShouldAcceptRequestsFrom127001()
        {
            // Given
            int port = GetEphemeralPort();
            Server server = new Server(TestContext.CurrentContext.TestDirectory, port);
            await server.StartAsync();

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri($"http://127.0.0.1:{port}/index.html")
            };
            HttpResponseMessage response = await client.GetAsync("/");

            // When, Then
            response.IsSuccessStatusCode.ShouldBeTrue();
            server.Dispose();
        }

        [Test]
        public async Task ServerShouldAcceptRequestsFromLocalhost()
        {
            // Given
            int port = GetEphemeralPort();
            Server server = new Server(TestContext.CurrentContext.TestDirectory, port);
            await server.StartAsync();

            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{port}/")
            };
            HttpResponseMessage response = await client.GetAsync("/");

            // When, Then
            response.IsSuccessStatusCode.ShouldBeTrue();
            server.Dispose();
        }
    }
}