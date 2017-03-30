using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Hosting.LiveReload;
using Wyam.Testing;

namespace Wyam.Hosting.Tests
{
    [TestFixture]
    public class ServerFixture : BaseFixture
    {
        public class TriggerReloadTests : ServerFixture
        {
            [Test]
            public void RebuildCompletedShouldNotifyConnectedClients()
            {
                // Given
                IReloadClient reloadClientMock = Substitute.For<IReloadClient>();
                reloadClientMock.IsConnected.Returns(true);
                Server server = Substitute.ForPartsOf<Server>(string.Empty, 35729);
                server.LiveReloadClients.Returns(new ConcurrentBag<IReloadClient> { reloadClientMock });

                // When
                server.TriggerReload();

                // Then
                reloadClientMock.Received().NotifyOfChanges();
            }

            [Test]
            public void RebuildCompletedShouldAvoidMissingClients()
            {
                // Given
                IReloadClient reloadClientMock = Substitute.For<IReloadClient>();
                reloadClientMock.IsConnected.Returns(false);
                Server server = Substitute.ForPartsOf<Server>(string.Empty, 35729);
                server.LiveReloadClients.Returns(new ConcurrentBag<IReloadClient> { reloadClientMock });

                // When
                server.TriggerReload();

                // Then
                reloadClientMock.DidNotReceive().NotifyOfChanges();
            }
        }

        public class EndpointTests : ServerFixture
        {
            [Test]
            public void ServerShouldBindWithoutUrlReservations()
            {
                // Given
                int port = GetEphemeralPort();
                Server server = null;

                // When, Then
                Assert.DoesNotThrow(() => server = new Server(string.Empty, port));
                server?.Dispose();
            }
        }

        public class HostnameTests : ServerFixture
        {
            [Test]
            public void ServerShouldBindWithoutUrlReservations()
            {
                // Given
                int port = GetEphemeralPort();
                Server server = new Server(string.Empty, port);

                // When, Then
                Assert.DoesNotThrow(() => server.Start());
                server.Dispose();
            }

            [Test]
            public async Task ServerShouldAcceptRequestsFromLocalhost()
            {
                // Given
                int port = GetEphemeralPort();
                Server server = new Server(string.Empty, port);
                server.Start();

                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri($"http://localhost:{port}/")
                };
                HttpResponseMessage response = await client.GetAsync("livereload.js");

                // When, Then
                Assert.IsTrue(response.IsSuccessStatusCode);
                server.Dispose();
            }

            [Test]
            public async Task ServerShouldAcceptRequestsFrom127001()
            {
                // Given
                int port = GetEphemeralPort();
                Server server = new Server(string.Empty, port);
                server.Start();

                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri($"http://127.0.0.1:{port}/")
                };
                HttpResponseMessage response = await client.GetAsync("livereload.js");

                // When, Then
                Assert.IsTrue(response.IsSuccessStatusCode);
                server.Dispose();
            }
        }

        private static int GetEphemeralPort()
        {
            // Based on http://stackoverflow.com/a/150974/2001966
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}