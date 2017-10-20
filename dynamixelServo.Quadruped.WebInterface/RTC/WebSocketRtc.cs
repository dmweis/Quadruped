using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamixelServo.Quadruped;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Remotion.Linq.Clauses;

namespace dynamixelServo.Quadruped.WebInterface.RTC
{
    public class WebSocketRtc
    {
        private readonly RequestDelegate _next;
        private readonly RobotController _robotController;
        private readonly IApplicationLifetime _applicationLifetime;

        public WebSocketRtc(RequestDelegate next, RobotController robotController, IApplicationLifetime applicationLifetime)
        {
            _next = next;
            _robotController = robotController;
            _applicationLifetime = applicationLifetime;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await WebSocketHandlerTask(webSocket, _applicationLifetime.ApplicationStopped);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task WebSocketHandlerTask(WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
        {
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var message = await webSocket.ReceiveObjectAsync<RobotMessage>(cancellationToken);
                    const float deadzone = 0.3f;
                    if (message.JoystickType == JoystickType.Direction)
                    {
                        _robotController.Move(message.CalculateHeadingVector(deadzone));
                    }
                    else
                    {
                        _robotController.Rotate(message.GetScaledX(deadzone, 2, 25));
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                }
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of connection", cancellationToken);
        }
    }

    static class WebSocketExtensions
    {
        public static Task SendObjectAsync<T>(this WebSocket webSocket, T message, CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            return webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, cancellationToken);
        }

        public static async Task<T> ReceiveObjectAsync<T>(this WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<byte> incoming = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                byte[] buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new IOException($"CLient disconected. Status: {result?.CloseStatus} Description: {result.CloseStatusDescription}");
                }
                incoming.AddRange(buffer.Take(result.Count));
            } while (!result.EndOfMessage);
            var message = Encoding.UTF8.GetString(incoming.ToArray());
            return JsonConvert.DeserializeObject<T>(message);
        }

        public static async Task<string> ReceiveStringAsync(this WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<byte> incoming = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                byte[] buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new IOException($"CLient disconected. Status: {result?.CloseStatus} Description: {result.CloseStatusDescription}");
                }
                incoming.AddRange(buffer.Take(result.Count));
            } while (!result.EndOfMessage);
            var message = Encoding.UTF8.GetString(incoming.ToArray());
            return message;
        }
    }

    static class RTCExtensions
    {
        public static IApplicationBuilder UseWebSocketRtc(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketRtc>();
        }
    }
}
