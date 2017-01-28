using System.Collections.Generic;

namespace Wyam.LiveReload.Messages
{
    internal class HelloMessage : ILiveReloadMessage
    {
        public ICollection<string> Protocols { get; set; }

        public string ServerName { get; set; } = "Wyam";

        public string Command { get; set; } = "hello";
    }
}