using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class VideoStreamingService : IVideoService
    {
        public bool StreamRunning { get; private set; }

        public VideoStreamingService(IApplicationLifetime applicationLifetime, ILogger<VideoStreamingService> logger)
        {
            applicationLifetime.ApplicationStopping.Register(KillStream);
            int port = 8080;
            logger.LogInformation("Starting video server");
            BashCommand
                .Command($"nohup mjpg_streamer -o \"output_http.so -p {port}\" -i \"input_raspicam.so -x 1280 -y 720 -fps 10\" &")
                .Execute();
            StreamRunning = true;
            logger.LogInformation("Video server up and running");
        }

        public void KillStream()
        {
            BashCommand
                .Command("pkill mjpg_streamer")
                .Execute();
            StreamRunning = false;
        }
    }
}
