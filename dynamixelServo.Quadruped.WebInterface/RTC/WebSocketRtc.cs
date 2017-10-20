using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamixelServo.Quadruped;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace dynamixelServo.Quadruped.WebInterface.RTC
{
    public class WebSocketRtc
    {
        private readonly RequestDelegate _next;
        private readonly RobotController _robotController;

        public WebSocketRtc(RequestDelegate next, RobotController robotController)
        {
            _next = next;
            _robotController = robotController;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await WebSocketHandlerTask(webSocket);
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

        private async Task WebSocketHandlerTask(WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var message = await webSocket.ReceiveObjectAsync<RobotMessage>();
                    const float deadzone = 0.4f;
                    if (message.JoystickType == JoystickType.Direction)
                    {
                        _robotController.Move(message.CalculateHeadingVector(deadzone));
                    }
                    else
                    {
                        _robotController.Rotate(message.CalculateHeadingVector(deadzone));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of connection", CancellationToken.None);
        }
    }

    static class WebSocketExtensions
    {
        public static Task SendObjectAsync<T>(this WebSocket webSocket, T message)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            return webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<T> ReceiveObjectAsync<T>(this WebSocket webSocket)
        {
            List<byte> incoming = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                byte[] buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                incoming.AddRange(buffer.Take(result.Count));
            } while (!result.EndOfMessage);
            var message = Encoding.UTF8.GetString(incoming.ToArray());
            return JsonConvert.DeserializeObject<T>(message);
        }

        public static async Task<string> ReceiveStringAsync(this WebSocket webSocket)
        {
            List<byte> incoming = new List<byte>();
            WebSocketReceiveResult result;
            do
            {
                byte[] buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
