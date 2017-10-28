using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class VideoStreamingService : IVideoService
    {
        public bool StreamRunning { get; private set; }
        private readonly Process _streamerProcess;

        public VideoStreamingService(IApplicationLifetime applicationLifetime, ILogger<VideoStreamingService> logger)
        {
            applicationLifetime.ApplicationStopping.Register(KillStream);

            const int port = 8080;
            const int horizontalResolution = 800;
            const int verticalResolution = 600;
            const int framerate = 10;

            logger.LogInformation("Starting video server");
            _streamerProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/local/bin/mjpg_streamer",
                    Arguments = $" -o \"output_http.so -p {port}\" -i \"input_raspicam.so -x {horizontalResolution} -y {verticalResolution} -fps {framerate}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _streamerProcess.Start();
            StreamRunning = true;
            logger.LogInformation("Video server up and running");
        }

        public void KillStream()
        {
            _streamerProcess.Close();
            StreamRunning = false;
        }
    }
}
