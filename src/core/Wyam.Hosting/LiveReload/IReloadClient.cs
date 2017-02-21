namespace Wyam.Hosting.LiveReload
{
    internal interface IReloadClient
    {
        bool IsConnected { get; }
        void NotifyOfChanges();
    }
}