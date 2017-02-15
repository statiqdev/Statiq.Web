namespace Wyam.LiveReload.Messages
{
    internal class BasicMessage : ILiveReloadMessage
    {
        public string Command { get; set; }
    }
}