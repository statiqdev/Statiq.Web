namespace Wyam.LiveReload.Messages
{
    internal class ReloadMessage : ILiveReloadMessage
    {
        public string Path { get; set; }

        public bool LiveCss { get; set; }

        public string Command { get; set; } = "reload";
    }
}