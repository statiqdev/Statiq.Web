using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Web.Hosting.LiveReload.Messages;

namespace Statiq.Web.Hosting.LiveReload
{
    internal class LiveReloadServer
    {
        private readonly ConcurrentBag<LiveReloadSocket> _sockets = new ConcurrentBag<LiveReloadSocket>();

        public async Task ConnectAsync(WebSocket webSocket, CancellationToken ct = default(CancellationToken))
        {
            LiveReloadSocket socket = new LiveReloadSocket(webSocket);
            _sockets.Add(socket);
            await socket.ReceiveMessagesAsync();
        }

        public async Task SendReloadMessageAsync() => await SendMessageAsync(new ReloadMessage());

        public async Task SendMessageAsync<TMessage>(TMessage message)
            where TMessage : ILiveReloadMessage
        {
            string json = JsonSerializer.Serialize(message, LiveReloadSocket.DefaultJsonSerializerOptions);
            byte[] bytes = Encoding.UTF8.GetBytes(json); // UTF-8 by spec
            ArraySegment<byte> segment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            foreach (LiveReloadSocket socket in _sockets.Where(x => x.IsConnected))
            {
                await socket.SendMessageAsync(segment);
            }
        }
    }
}