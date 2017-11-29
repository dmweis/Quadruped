using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quadruped.WebInterface.RobotController;
using Quadruped.WebInterface.VideoStreaming;

namespace Quadruped.WebInterface.RTC
{
    public class WebSocketRtc
    {
        private readonly RequestDelegate _next;
        private readonly IRobot _robot;
        private readonly ICameraController _cameraController;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<WebSocket> _clients = new ConcurrentBag<WebSocket>();

        public WebSocketRtc(RequestDelegate next, IRobot robot, ICameraController cameraController, IApplicationLifetime applicationLifetime, ILogger<WebSocketRtc> logger)
        {
            _next = next;
            _robot = robot;
            _cameraController = cameraController;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _robot.NewTelemetricsUpdate += (sender, telemetrics) => EmitToAll("telemetrics", telemetrics).ConfigureAwait(false);
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

        public async Task EmitToAll<T>(string topic, T message)
        {
            foreach (var client in _clients)
            {
                await client.SendObjectAsync(new { topic, message }, _applicationLifetime.ApplicationStopped);
            }
        }

        private async Task WebSocketHandlerTask(WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
        {
            _clients.Add(webSocket);
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var message = await webSocket.ReceiveObjectAsync<RobotMessage>(cancellationToken);
                    const float deadzone = 0.3f;
                    switch (message.JoystickType)
                    {
                        case JoystickType.Direction:
                            _robot.Direction = message.CalculateHeadingVector(deadzone);
                            break;
                        case JoystickType.Rotation:
                            _robot.Rotation = message.GetScaledX(deadzone, 2, 25);
                            break;
                        case JoystickType.Camera:
                            if (message.MessageType == MessageType.Reset)
                            {
                                _cameraController.CenterView();
                            }
                            else
                            {
                                _cameraController.StartMove(message.CalculateHeadingVector(deadzone));
                            }
                            break;
                        //default:
                        //    throw new NotImplementedException();
                    }
                }
                catch (IOException e)
                {
                    _logger.LogInformation("Clinet disconnected", e);
                    break;
                }
                catch (WebSocketException e)
                {
                    _logger.LogWarning($"Client disconnected. Error code: {e.WebSocketErrorCode}");
                    break;
                }
                catch (Exception e)
                {
                    _logger.LogError("WebSocket threw exception", e);
                    break;
                }
            }
            while (!_clients.TryTake(out webSocket))
            {
                _logger.LogError("Failed to remove client from bag");
            }
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "End of connection", cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError("failed disconnecting from client", e);
            }
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
                    throw new IOException($"Client disconected. Status: {result?.CloseStatus} Description: {result.CloseStatusDescription}");
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
