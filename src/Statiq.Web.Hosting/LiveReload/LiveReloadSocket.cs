using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Web.Hosting.LiveReload.Messages;

namespace Statiq.Web.Hosting.LiveReload
{
    internal class LiveReloadSocket
    {
        internal static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
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
                    await stream.WriteAsync(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);
                return stream.ToArray();
            }
        }

        private async Task HandleMessageAsync(byte[] message)
        {
            string json = Encoding.UTF8.GetString(message);
            BasicMessage parsedMessage = JsonSerializer.Deserialize<BasicMessage>(json, DefaultJsonSerializerOptions);
            switch (parsedMessage.Command)
            {
                case "info":
                    HandleInfo(json);
                    break;

                case "hello":
                    await HandleHelloAsync(json);
                    break;

                default:
                    // Unknown message, just ignore it
                    break;
            }
        }

        private void HandleInfo(string json)
        {
            InfoMessage info = JsonSerializer.Deserialize<InfoMessage>(json, DefaultJsonSerializerOptions);

            // noop
        }

        private async Task HandleHelloAsync(string json)
        {
            HelloMessage hello = JsonSerializer.Deserialize<HelloMessage>(json, DefaultJsonSerializerOptions);

            string negotiatedProtocol = hello
                .Protocols
                .Intersect(SupportedProtocols)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (negotiatedProtocol is null)
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
            string json = JsonSerializer.Serialize(message, DefaultJsonSerializerOptions);
            byte[] bytes = Encoding.UTF8.GetBytes(json); // UTF-8 by spec
            ArraySegment<byte> segment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await SendMessageAsync(segment);
        }

        public async Task SendMessageAsync(ArraySegment<byte> segment) =>
            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}