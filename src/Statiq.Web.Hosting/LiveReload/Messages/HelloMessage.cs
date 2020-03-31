using System.Collections.Generic;

namespace Statiq.Web.Hosting.LiveReload.Messages
{
    internal class HelloMessage : ILiveReloadMessage
    {
        public ICollection<string> Protocols { get; set; }

        public string ServerName { get; set; } = "Statiq";

        public string Command { get; set; } = "hello";

        public HelloMessage(ICollection<string> protocols)
        {
            Protocols = protocols;
        }

        public HelloMessage()
        {
        }
    }
}