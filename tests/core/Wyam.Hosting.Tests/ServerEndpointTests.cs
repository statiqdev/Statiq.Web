using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Wyam.Hosting.LiveReload;

namespace Wyam.Hosting.Tests
{
    [TestFixture(Category = "ExcludeFromAppVeyor")]
    public class ServerEndpointTests
    {
        [Test]
        public void ServerShouldBindWithoutUrlReservations()
        {
            int port = GetEphemeralPort();
            Server server = null;
            Assert.DoesNotThrow(() => server = new Server("", port));
            server?.Dispose();
        }

        private static int GetEphemeralPort()
        {
            // Based on http://stackoverflow.com/a/150974/2001966
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}