namespace Wyam.LiveReload.Messages
{
    internal class InfoMessage : ILiveReloadMessage
    {
        public string Command { get; set; } = "info";

        public string Url { get; set; }
    }
}