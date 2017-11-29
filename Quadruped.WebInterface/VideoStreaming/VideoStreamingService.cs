using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Quadruped.WebInterface.VideoStreaming
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
            if (IsCorrectPlatform())
            {
                if (!StreamRunning)
                {
                    await Task.Run((Action)StartStream);
                }
            }
            else
            {
                _logger.LogError("Incorrect platform for video streamer");
            }
        }

        public async Task StopStream()
        {
            if (IsCorrectPlatform())
            {
                await Task.Run((Action)KillStream);
            }
            else
            {
                _logger.LogError("Incorrect platform for video streamer");
            }
        }

        public async Task RestartAsync()
        {
            if (IsCorrectPlatform())
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
            else
            {
                _logger.LogError("Incorrect platform for video streamer");
            }
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

        private void KillStream()
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

        private bool IsCorrectPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var architecture = RuntimeInformation.OSArchitecture;
                return architecture == Architecture.Arm || architecture == Architecture.Arm64;
            }
            return false;
        }
    }
}
