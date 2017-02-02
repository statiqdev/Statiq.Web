using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Owin.WebSocket;

using Wyam.Common.Tracing;
using Wyam.LiveReload.Messages;

namespace Wyam.LiveReload
{
    public interface IReloadClient
    {
        bool IsConnected { get; }
        void NotifyOfChanges(string modifiedFile, bool supportCssReload = true);
    }

    public class ReloadClient : WebSocketConnection, IReloadClient
    {
        // Attempt to support the Livereload protocol v7.
        // http://feedback.livereload.com/knowledgebase/articles/86174-livereload-protocol

        private readonly JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };
        private readonly HashSet<string> _supportedVersion = new HashSet<string>
        {
            "http://livereload.com/protocols/official-7" // Only supporting v7 right now
        };
        private readonly Guid _clientId = Guid.NewGuid();

        public bool IsConnected { get; private set; }

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
            LogVerbose($"Lost connection with LiveReload client. Status=({closeStatus}) Description=({closeStatusDescription})");
            base.OnClose(closeStatus, closeStatusDescription);
        }

        public void NotifyOfChanges(string modifiedFile, bool supportCssReload = true)
        {
            // Asume changes have been rebuilt by this time
            ReloadMessage reloadMessage = new ReloadMessage
            {
                Path = modifiedFile,
                LiveCss = supportCssReload
            };

            LogVerbose($"Sending reload for {modifiedFile}.");
            SendObject(reloadMessage);
        }

        private ILiveReloadMessage HandleClientMessage(string json)
        {
            BasicMessage parsedMessage = JsonConvert.DeserializeObject<BasicMessage>(json, _defaultSettings);
            switch (parsedMessage.Command)
            {
                case "info":
                    InfoMessage info = JsonConvert.DeserializeObject<InfoMessage>(json, _defaultSettings);
                    LogVerbose($"LiveReload client sent info ({info.Url}).");
                    break;
                case "hello":
                    HelloMessage hello = JsonConvert.DeserializeObject<HelloMessage>(json, _defaultSettings);
                    HandleHello(hello);
                    break;
                default:
                    LogVerbose($"Unknown command recieved from LiveReload client = {parsedMessage.Command}.");
                    break;
            }

            return parsedMessage;
        }

        private void HandleHello(HelloMessage message)
        {
            string negotiatedVersion = message.Protocols
                                           .Intersect(_supportedVersion)
                                           .OrderByDescending(x => x)
                                           .FirstOrDefault();

            if (negotiatedVersion == null)
            {
                string incompatibleMessage = "LiveReload client is not compatible with this server, aborting connection. " +
                                          $"Client=({string.Join(",", message.Protocols)}) " +
                                          $"Server=({string.Join(",", _supportedVersion)})";
                LogVerbose(incompatibleMessage);
                Abort();
            }
            else
            {
                LogVerbose($"LiveReload client hello. Negotiated=({negotiatedVersion})");
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

        private void LogVerbose(string message)
        {
            Trace.Verbose($"{message} Client=({_clientId})");
        }
    }
}