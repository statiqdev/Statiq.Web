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
    internal class LiveReloadSocket
    {
        internal static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        private static readonly HashSet<string> SupportedProtocols = new HashSet<string>
        {
            "http://livereload.com/protocols/official-7" // Only supporting v7 right now
        };

        private readonly WebSocket _webSocket;

        private bool _isConnected;  // Received hello message and not canceled

        public LiveReloadSocket(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public bool IsConnected => _webSocket.State == WebSocketState.Open && _isConnected;

        public async Task ReceiveMessagesAsync(CancellationToken ct = default(CancellationToken))
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    byte[] message = await ReceiveMessageAsync(ct);
                    await HandleMessageAsync(message);
                }
                catch (OperationCanceledException)
                {
                    _isConnected = false;
                }
            }
        }

        private async Task<byte[]> ReceiveMessageAsync(CancellationToken ct = default(CancellationToken))
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[8192]);
            using (MemoryStream stream = new MemoryStream())
            {
                // Read the message in chunks until it's complete
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();
                    result = await _webSocket.ReceiveAsync(buffer, ct);
                    stream.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);
                return stream.ToArray();
            }
        }

        private async Task HandleMessageAsync(byte[] message)
        {
            string json = Encoding.UTF8.GetString(message);
            BasicMessage parsedMessage = JsonConvert.DeserializeObject<BasicMessage>(json, DefaultJsonSerializerSettings);
            switch (parsedMessage.Command)
            {
                case "info":
                    HandleInfo(json);
                    break;

                case "hello":
                    await HandleHello(json);
                    break;

                default:
                    // Unknown message, just ignore it
                    break;
            }
        }

        private void HandleInfo(string json)
        {
            InfoMessage info = JsonConvert.DeserializeObject<InfoMessage>(json, DefaultJsonSerializerSettings);

            // noop
        }

        private async Task HandleHello(string json)
        {
            HelloMessage hello = JsonConvert.DeserializeObject<HelloMessage>(json, DefaultJsonSerializerSettings);

            string negotiatedProtocol = hello
                .Protocols
                .Intersect(SupportedProtocols)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (negotiatedProtocol == null)
            {
                string incompatibleMessage =
                    "No compatible LiveReload protocols found, aborting connection " +
                    $"(client: {string.Join(", ", hello.Protocols)}, " +
                    $"server: {string.Join(", ", SupportedProtocols)})";
                _isConnected = false;
                await _webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, incompatibleMessage, CancellationToken.None);
                return;
            }

            // If we support the protocol, send hello and consider this socket connected
            await SendHelloMessageAsync();
            _isConnected = true;
        }

        private async Task SendHelloMessageAsync() => await SendMessageAsync(new HelloMessage(SupportedProtocols));

        public async Task SendMessageAsync<TMessage>(TMessage message)
            where TMessage : ILiveReloadMessage
        {
            string json = JsonConvert.SerializeObject(message, DefaultJsonSerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json); // UTF-8 by spec
            ArraySegment<byte> segment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await SendMessageAsync(segment);
        }

        public async Task SendMessageAsync(ArraySegment<byte> segment) =>
            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}