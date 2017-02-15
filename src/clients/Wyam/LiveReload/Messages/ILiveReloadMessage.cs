namespace Wyam.LiveReload.Messages
{
    internal interface ILiveReloadMessage
    {
        string Command { get; set; }
    }
}