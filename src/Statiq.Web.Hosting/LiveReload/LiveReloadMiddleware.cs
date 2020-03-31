using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Statiq.Web.Hosting.LiveReload
{
    internal class LiveReloadMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LiveReloadServer _liveReloadServer;

        public LiveReloadMiddleware(RequestDelegate next, LiveReloadServer liveReloadServer)
        {
            _next = next;
            _liveReloadServer = liveReloadServer;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/livereload")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await _liveReloadServer.ConnectAsync(webSocket, context.RequestAborted);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }

            await _next(context);
        }
    }
}