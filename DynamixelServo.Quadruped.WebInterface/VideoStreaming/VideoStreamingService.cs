using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class VideoStreamingService : IVideoService
    {
        private const int Port = 8080;

        public bool StreamRunning { get; private set; }
        public StreamerConfig StreamerConfiguration { get; set; }

        private Process _streamerProcess;
        private readonly ILogger<VideoStreamingService> _logger;

        public VideoStreamingService(IApplicationLifetime applicationLifetime, ILogger<VideoStreamingService> logger, IOptions<StreamerConfig> config)
        {
            applicationLifetime.ApplicationStopping.Register(KillStream);
            _logger = logger;
            StreamerConfiguration = config.Value;
        }

        public async Task EnsureStreamOn()
        {
            if (!StreamRunning)
            {
                await Task.Run((Action)StartStream);
            }
        }

        public async Task RestartAsync()
        {
            await Task.Run(() =>
            {
                if (StreamRunning)
                {
                    KillStream();
                }
                StartStream();
            });
        }

        private void StartStream()
        {
            _logger.LogInformation("Starting video server");
            _streamerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/local/bin/mjpg_streamer",
                    Arguments = $" -o \"output_http.so -p {Port}\" -i \"input_raspicam.so -x {StreamerConfiguration.HorizontalResolution} -y {StreamerConfiguration.VerticalResolution} -fps {StreamerConfiguration.Framerate} -quality {StreamerConfiguration.ImageQuality}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _streamerProcess.Start();
            StreamRunning = true;
            _logger.LogInformation("Video server up and running");
        }

        public void KillStream()
        {
            _logger.LogInformation("Shutting down streaming server");
            var killProcessStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = " -c \"pkill -SIGINT mjpg_streamer\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var killProcess = new Process { StartInfo = killProcessStartInfo })
            {
                killProcess.Start();
                killProcess.WaitForExit();
            }
            _streamerProcess.WaitForExit();
            _streamerProcess.Dispose();
            StreamRunning = false;
            _logger.LogInformation("Streaming shut down");
        }
    }
}
