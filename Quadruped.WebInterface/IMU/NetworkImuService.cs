using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Quadruped.WebInterface.IMU
{
    public class NetworkImuService : IImuService
    {
        public event EventHandler<ImuData> NewImuData;

        private readonly ILogger _logger;

        private const string HostName = "localhost";
        private const int PortName = 4242;
        private const int ReadTimeout = 15;
        private const byte LineSeparator = (byte)'\n';

        private bool _keepGoing = true;

        public NetworkImuService(IApplicationLifetime applicationLifetime, ILogger<NetworkImuService> logger)
        {
            applicationLifetime.ApplicationStopping.Register(() => _keepGoing = false);
            _logger = logger;
            Task.Run((Action)HandleConnection);
        }

        private async void HandleConnection()
        {
            try
            {
                List<byte> buffer = new List<byte>();
                using (var client = new TcpClient(HostName, PortName))
                using (var stream = client.GetStream())
                {
                    client.ReceiveTimeout = ReadTimeout;
                    while (_keepGoing)
                    {
                        var byteBuffer = new byte[client.Available];
                        var countRead = await stream.ReadAsync(byteBuffer, 0, byteBuffer.Length);
                        buffer.AddRange(byteBuffer.Take(countRead));
                        // look for separator
                        var firstIndex = buffer.IndexOf(LineSeparator);
                        // MOre efficient than iteratig twice with a contains
                        if (firstIndex != -1)
                        {
                            if (firstIndex > 0)
                            {
                                RaiseNewImuData(Encoding.UTF8.GetString(buffer.Take(firstIndex).ToArray()));
                                buffer.RemoveRange(0, firstIndex + 1);
                            }
                            else
                            {
                                // This is the case if new line is the first character
                                // This won't happen a lot
                                buffer.Remove(LineSeparator);
                            }
                        }
                    }
                }
            }
            catch (Exception e) when (e is IOException || e is SocketException)
            {
               _logger.LogError("Connection to IMU server failed", e);
            }
        }

        private void RaiseNewImuData(string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<ImuData>(json);
                NewImuData?.Invoke(this, message);
            }
            catch (Exception e) when (e is JsonReaderException || e is JsonSerializationException)
            {
                _logger.LogError("Json deserealize error in NetworkImuService", e);
            }
        }
    }
}
