namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class VideoStreamingService
    {

        public string StreamPath { get; set; }

        public VideoStreamingService()
        {
            var result = BashCommand
                .Command("ls")
                .AddCommand("mkdir TestFolder")
                .AddCommand("cd TestFolder")
                .AddCommand("echo 'Test test test ' > test.txt")
                .Execute();
        }

        public void StartStream(int port)
        {
            BashCommand
                .Command($"mjpg_streamer -o \"output_http.so -p {port}\" -i \"input_raspicam.so -x 1280 -y 720 -fps 10\"")
                .Execute();
            var ip = BashCommand.Command("hostname -i").Execute();
            StreamPath = $"http://{ip}:{port}/?action=stream";
        }

        public void KillStream()
        {
            BashCommand
                .Command("pkill mjpg_streamer")
                .Execute();
            StreamPath = string.Empty;
        }
    }
}
