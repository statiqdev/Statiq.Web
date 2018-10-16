using Wyam.Hosting.LiveReload;

namespace Wyam.Hosting.Tests
{
    public class TestReloadClient : IReloadClient
    {
        public bool IsConnected { get; set; }

        public int NotifyOfChangesCount { get; set; }

        public void NotifyOfChanges() => NotifyOfChangesCount++;
    }
}