using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Wyam.Hosting.LiveReload;

namespace Wyam.Hosting.Tests.LiveReload
{
    [TestFixture(Category = "ExcludeFromAppVeyor")]
    public class LiveReloadServerHostnameTests
    {
        [Test]
        public void ServerShouldBindWithoutUrlReservations()
        {
            int port = GetEphemeralPort();
            LiveReloadServer server = null;
            Assert.DoesNotThrow(() => server = new LiveReloadServer(port, null));
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