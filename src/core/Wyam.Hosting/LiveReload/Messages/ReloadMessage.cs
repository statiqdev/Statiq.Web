namespace Wyam.Hosting.LiveReload.Messages
{
    internal class ReloadMessage : ILiveReloadMessage
    {
        public string Path { get; } = string.Empty;  // Always reload whatever page the client is on

        public bool LiveCss { get; } = true;

        public string Command { get; set; } = "reload";
    }
}