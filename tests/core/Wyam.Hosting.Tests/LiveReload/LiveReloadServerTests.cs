using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Wyam.Hosting.LiveReload;

namespace Wyam.Hosting.Tests.LiveReload
{
    [TestFixture]
    public class LiveReloadServerTests
    {
        [Test]
        public void RebuildCompletedShouldNotifyConnectedClients()
        {
            IReloadClient reloadClientMock = Substitute.For<IReloadClient>();
            reloadClientMock.IsConnected.Returns(true);

            LiveReloadServer server = Substitute.ForPartsOf<LiveReloadServer>(35729, (Action<string>)(_ => { }));
            server.ReloadClients.Returns(new List<IReloadClient> {reloadClientMock});
            server.TriggerReload();

            reloadClientMock.Received().NotifyOfChanges();
        }

        [Test]
        public void RebuildCompletedShouldAvoidMissingClients()
        {
            IReloadClient reloadClientMock = Substitute.For<IReloadClient>();
            reloadClientMock.IsConnected.Returns(false);

            LiveReloadServer server = Substitute.ForPartsOf<LiveReloadServer>(35729, (Action<string>)(_ => { }));
            server.ReloadClients.Returns(new List<IReloadClient> {reloadClientMock});
            server.TriggerReload();

            reloadClientMock.DidNotReceive().NotifyOfChanges();
        }
    }
}