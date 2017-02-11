using System.Collections.Generic;
using System.Linq;

using NSubstitute;

using NUnit.Framework;

using Ploeh.AutoFixture;

using Wyam.LiveReload;

namespace Wyam.Tests.LiveReload
{
    [TestFixture]
    public class LiveReloadServerTests
    {
        private static readonly Fixture AutoFixture = new Fixture();

        [Test]
        public void RebuildCompletedShouldNotifyConnectedClients()
        {
            var changedFiles = AutoFixture.CreateMany<string>(1).ToList();

            var reloadClientMock = Substitute.For<IReloadClient>();
            reloadClientMock.IsConnected.Returns(true);

            var server = Substitute.ForPartsOf<LiveReloadServer>();
            server.ReloadClients.Returns(new List<IReloadClient> {reloadClientMock});
            server.RebuildCompleted(changedFiles);

            reloadClientMock.Received().NotifyOfChanges(Arg.Is<string>(s => changedFiles.Contains(s)), Arg.Is(true));
        }

        [Test]
        public void RebuildCompletedShouldAvoidMissingClients()
        {
            var changedFiles = AutoFixture.CreateMany<string>(1).ToList();

            var reloadClientMock = Substitute.For<IReloadClient>();
            reloadClientMock.IsConnected.Returns(false);

            var server = Substitute.ForPartsOf<LiveReloadServer>();
            server.ReloadClients.Returns(new List<IReloadClient> {reloadClientMock});
            server.RebuildCompleted(changedFiles);

            reloadClientMock.DidNotReceive().NotifyOfChanges(Arg.Any<string>(), Arg.Any<bool>());
        }

        [Test]
        public void RebuildCompletedShouldNotifyOfAllChangedFiles()
        {
            var changedFiles = AutoFixture.CreateMany<string>().ToList();

            var reloadClientMock = Substitute.For<IReloadClient>();
            reloadClientMock.IsConnected.Returns(true);

            var server = Substitute.ForPartsOf<LiveReloadServer>();
            server.ReloadClients.Returns(new List<IReloadClient> {reloadClientMock});
            server.RebuildCompleted(changedFiles);

            foreach (var changedFile in changedFiles)
            {
                reloadClientMock.Received().NotifyOfChanges(Arg.Is(changedFile), Arg.Is(true));
            }
        }
    }
}