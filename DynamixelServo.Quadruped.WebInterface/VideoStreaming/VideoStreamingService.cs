using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class VideoStreamingService : IVideoService
    {
        private const int Port = 8080;

        public bool StreamRunning { get; private set; }
        public StreamerConfig StreamerConfiguration { get; private set; }

        private Process _streamerProcess;

        public VideoStreamingService(IApplicationLifetime applicationLifetime, ILogger<VideoStreamingService> logger)
        {
            applicationLifetime.ApplicationStopping.Register(KillStream);
            logger.LogInformation("Starting video server");
            StartStream();
            logger.LogInformation("Video server up and running");
        }

        public Task RestartAsync(StreamerConfig config) => Task.Run(() => Restart(config));

        public void Restart(StreamerConfig config)
        {
            KillStream();
            StartStream(config);
        }

        private void StartStream()
        {
            StartStream(new StreamerConfig());
        }

        private void StartStream(StreamerConfig config)
        {
            StreamerConfiguration = config;
            _streamerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/local/bin/mjpg_streamer",
                    Arguments = $" -o \"output_http.so -p {Port}\" -i \"input_raspicam.so -x {config.HorizontalResolution} -y {config.VerticalResolution} -fps {config.Framerate} -quality {config.ImageQuality}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _streamerProcess.Start();
            StreamRunning = true;
        }

        public void KillStream()
        {
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
        }
    }

    public class StreamerConfig
    {
        public int ImageQuality { get; set; } = 85;
        public int HorizontalResolution { get; set; } = 800;
        public int VerticalResolution { get; set; } = 600;
        public int Framerate { get; set; } = 10;
    }
}
