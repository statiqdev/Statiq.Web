namespace Wyam.Hosting.LiveReload.Messages
{
    internal class ReloadMessage : ILiveReloadMessage
    {
        public string Command { get; set; } = "reload";
    }
}