using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Quadruped.WebInterface.IMU
{
    public class NetworkImuService : IImuService
    {
        public event EventHandler<ImuData> NewImuData;

        private bool _keepGoing = true;

        public NetworkImuService(IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(() => _keepGoing = false);
            Task.Run((Action)HandleConnection);
        }

        private async void HandleConnection()
        {
            const byte separator = (byte) '\n';
            List<byte> buffer  = new List<byte>();
            using (var client = new TcpClient("localhost", 4242))
            using (var stream = client.GetStream())
            {
                client.ReceiveTimeout = 15;
                while (_keepGoing)
                {
                    var byteBuffer = new byte[client.Available];
                    var countRead = await stream.ReadAsync(byteBuffer, 0, byteBuffer.Length);
                    buffer.AddRange(byteBuffer.Take(countRead));
                    if (buffer.Contains(separator))
                    {
                        var firstIndex = buffer.IndexOf(separator);
                        if (firstIndex > 0)
                        {
                            RaiseNewImuData(Encoding.UTF8.GetString(buffer.Take(firstIndex).ToArray()));
                            buffer = new List<byte>(buffer.Skip(firstIndex + 1));
                        }
                        else
                        {
                            buffer.Remove(separator);
                        }
                    }
                }
            }
        }

        private void RaiseNewImuData(string json)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<ImuData>(json);
                NewImuData?.Invoke(this, message);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
