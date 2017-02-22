using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                Server server = Substitute.ForPartsOf<Server>("", 35729);
                server.LiveReloadClients.Returns(new ConcurrentBag<IReloadClient> {reloadClientMock});

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
                Server server = Substitute.ForPartsOf<Server>("", 35729);
                server.LiveReloadClients.Returns(new ConcurrentBag<IReloadClient> {reloadClientMock});

                // When
                server.TriggerReload();

                // Then
                reloadClientMock.DidNotReceive().NotifyOfChanges();
            }
        }
    }
}