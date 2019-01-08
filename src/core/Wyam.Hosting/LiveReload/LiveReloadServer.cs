using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Wyam.Hosting.LiveReload.Messages;

namespace Wyam.Hosting.LiveReload
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
            string json = JsonConvert.SerializeObject(message, LiveReloadSocket.DefaultJsonSerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json); // UTF-8 by spec
            ArraySegment<byte> segment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            foreach (LiveReloadSocket socket in _sockets.Where(x => x.IsConnected))
            {
                await socket.SendMessageAsync(segment);
            }
        }
    }
}