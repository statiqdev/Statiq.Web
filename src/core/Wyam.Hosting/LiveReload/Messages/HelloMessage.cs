using System.Collections.Generic;

namespace Wyam.Hosting.LiveReload.Messages
{
    internal class HelloMessage : ILiveReloadMessage
    {
        public ICollection<string> Protocols { get; set; }

        public string ServerName { get; set; } = "Wyam";

        public string Command { get; set; } = "hello";

        public HelloMessage(ICollection<string> protocols)
        {
            Protocols = protocols;
        }
    }
}