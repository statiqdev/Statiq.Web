using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Owin.WebSocket;

using Wyam.Hosting.LiveReload.Messages;

namespace Wyam.Hosting.LiveReload
{
    // Attempt to support the Livereload protocol v7.
    // http://feedback.livereload.com/knowledgebase/articles/86174-livereload-protocol
    internal class ReloadClient : FleckWebSocketConnection, IReloadClient
    {
        private readonly Guid _clientId = Guid.NewGuid();

        private readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };

        private readonly HashSet<string> _supportedVersion = new HashSet<string>
        {
            "http://livereload.com/protocols/official-7" // Only supporting v7 right now
        };

        public bool IsConnected { get; private set; }

        public ILogger Logger { private get; set; }

        public override Task OnMessageReceived(ArraySegment<byte> message, WebSocketMessageType type)
        {
            string json = Encoding.UTF8.GetString(message.Array, message.Offset, message.Count);
            HandleClientMessage(json);
            return Task.CompletedTask;
        }

        public override void OnOpen()
        {
            SayHello();
            base.OnOpen();
        }

        public override void OnClose(WebSocketCloseStatus? closeStatus, string closeStatusDescription)
        {
            IsConnected = false;
            Log($"Lost connection with LiveReload client, status: {closeStatus}, description: {closeStatusDescription}");
            base.OnClose(closeStatus, closeStatusDescription);
        }

        public void NotifyOfChanges()
        {
            ReloadMessage reloadMessage = new ReloadMessage();
            Log($"Sending LiveReload reload message");
            SendObject(reloadMessage);
        }

        private ILiveReloadMessage HandleClientMessage(string json)
        {
            BasicMessage parsedMessage = JsonConvert.DeserializeObject<BasicMessage>(json, _defaultSettings);
            switch (parsedMessage.Command)
            {
                case "info":
                    InfoMessage info = JsonConvert.DeserializeObject<InfoMessage>(json, _defaultSettings);
                    Log($"LiveReload client sent info: {info.Url}");
                    break;
                case "hello":
                    HelloMessage hello = JsonConvert.DeserializeObject<HelloMessage>(json, _defaultSettings);
                    HandleHello(hello);
                    break;
                default:
                    Log($"Unknown command received from LiveReload client: {parsedMessage.Command}");
                    break;
            }

            return parsedMessage;
        }

        private void HandleHello(HelloMessage message)
        {
            string negotiatedVersion = message
                .Protocols
                .Intersect(_supportedVersion)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (negotiatedVersion == null)
            {
                string incompatibleMessage =
                    "LiveReload client is not compatible with this server, aborting connection " +
                    $"(client: {string.Join(",", message.Protocols)}, " +
                    $"server: {string.Join(",", _supportedVersion)})";
                Log(incompatibleMessage);
                Abort();
            }
            else
            {
                Log($"LiveReload client hello, negotiated version {negotiatedVersion}");
                IsConnected = true;
            }
        }

        private void SayHello()
        {
            HelloMessage helloMessage = new HelloMessage
            {
                Protocols = _supportedVersion
            };
            SendObject(helloMessage);
        }

        private void SendObject<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj, _defaultSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json); // UTF-8 by spec
            SendText(bytes, true);
        }

        private void Log(string message) => Logger?.LogDebug($"{message} (LiveReload client: {_clientId})");
    }
}