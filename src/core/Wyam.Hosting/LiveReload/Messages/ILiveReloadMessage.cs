namespace Wyam.Hosting.LiveReload.Messages
{
    internal interface ILiveReloadMessage
    {
        string Command { get; set; }
    }
}